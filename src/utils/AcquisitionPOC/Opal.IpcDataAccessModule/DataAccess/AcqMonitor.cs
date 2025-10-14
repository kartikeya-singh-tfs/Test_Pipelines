using System;
using Microsoft.Extensions.Logging;
using ThermoFisher.Foundation.Acquisition;

namespace Thermofisher.Opal.IpcDataAccessModule.DataAccess
{
    ///<summary>
    /// Monitors acquisition events and manages connection to the acquisition service.
    /// </summary>
    public class AcqMonitor : IAcqMonitor
    {
        private readonly AcquisitionMonitor _acquisitionMonitor;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private IAcquisition _acquisitionService;
        private string _computerName = string.Empty;
        private int _tcpPort = -1;
        private bool _isDisposed;

        /// <summary>
        /// Event raised when a sample is being acquired.
        /// </summary>
        public event Action<RunStateEventArgs, AcquisitionSample> SampleAcquire = delegate { };

        /// <summary>
        /// Event raised when a sample acquisition is complete.
        /// </summary>
        public event Action<RunEventEventArgs, AcquisitionSample> SampleComplete = delegate { };

        /// <summary>
        /// Initializes a new instance of the <see cref="AcqMonitor"/> class.
        /// </summary>
        public AcqMonitor(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger("AcqMonitor");
            _acquisitionMonitor = new AcquisitionMonitor();
        }

        /// <summary>
        /// Connects to the acquisition service.
        /// </summary>
        /// <param name="computerName">Name of the computer (use "localhost" for the current PC).</param>
        /// <param name="tcpPort">TCP port to use, -1 for default.</param>
        /// <returns>True if the connection is successful; otherwise, false.</returns>
        public bool EstablishAcquisitionServiceConnection(string computerName, int tcpPort)
        {
            var result = false;

            try
            {
                // If the caller didn't supply a Computer Name then assume LOCALHOST
                _computerName = string.IsNullOrEmpty(computerName) ? "localhost" : computerName;
                // If the TCP Port wasn't specified by the caller then use the Default Port
                _tcpPort = tcpPort == -1 ? AcquisitionService.DefaultPort : tcpPort;

                _logger?.LogInformation($"computer name: {_computerName}, tcp port: {_tcpPort}");

                // Connect to server
                _acquisitionService = AcquisitionService.GetServer(_computerName);

                if (_acquisitionService == null)
                {
                    _logger?.LogInformation(
                        "AcquisitionService.GetServer returned NULL, acquisition service is not available"
                    );
                }
                else
                {
                    _logger?.LogInformation("Connected to the Acquisition Server");

                    // Hook up the Event Notification Mechanism
                    var isHooked = _acquisitionMonitor.Connect(_acquisitionService, true);
                    _logger?.LogInformation(
                        isHooked
                            ? "Connected to Acquisition Monitor"
                            : "Not able to connect to Acquisition Monitor"
                    );

                    result = isHooked;
                }
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "EstablishAcquisitionServiceConnection error");
            }

            return result;
        }

        /// <summary>
        /// Subscribes to acquisition events.
        /// NOTE: All acquisition events are blocking. The acquisition service execution will be blocked
        /// until the client's event handler completes execution/returns.
        /// </summary>
        public void Subscribe()
        {
            if (_acquisitionMonitor == null || !_acquisitionMonitor.IsConnected)
            {
                _logger?.LogInformation("acquisition monitor is not connected");
                return;
            }

            // monitoring acquisitions
            _acquisitionMonitor.RunEventNotify += AcquisitionMonitorOnRunEventNotify;
            _acquisitionMonitor.RunStateChanged += AcquisitionMonitorOnRunStateChanged;
            _logger?.LogInformation("Subscribe acquisition events");
        }

        /// <summary>
        /// Unsubscribes from acquisition events.
        /// </summary>
        public void Unsubscribe()
        {
            if (_acquisitionMonitor == null || !_acquisitionMonitor.IsConnected)
            {
                _logger?.LogInformation("acquisition monitor is not connected");
                return;
            }

            // monitoring acquisitions
            _acquisitionMonitor.RunEventNotify -= AcquisitionMonitorOnRunEventNotify;
            _acquisitionMonitor.RunStateChanged -= AcquisitionMonitorOnRunStateChanged;
            _logger?.LogInformation("Unsubscribe acquisition events");
        }

        /// <summary>
        /// Disconnects from the acquisition service and unsubscribes from events.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _logger?.LogInformation("Disconnecting....");
                Unsubscribe();
                _acquisitionMonitor?.Disconnect();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Disconnect error");
            }
            finally
            {
                AcquisitionService.Disconnect(_computerName, _tcpPort);
                _acquisitionService = null;
            }
        }

        /// <summary>
        /// Handles the RunStateChanged event from the acquisition monitor.
        /// Raises the SampleAcquire event when a sample is being acquired.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Run state event arguments.</param>
        private void AcquisitionMonitorOnRunStateChanged(object sender, RunStateEventArgs e)
        {
            var runState = e.RunState;
            var msg = $"[AcquisitionMonitorOnRunStateChanged] - {runState.RunState}";

            switch (runState.RunState)
            {
                //case RunState.PostRun:
                case RunState.Acquire:
                    var sample = ShowSampleStatus(
                        e.InstrumentName,
                        runState.SequenceId,
                        runState.SampleId,
                        msg
                    );
                    SampleAcquire.RaiseEvent(e, sample, _logger);
                    break;

                default:
                    _logger?.LogInformation(
                        $"[AcquisitionMonitorOnRunStateChanged] - {runState.RunState}"
                    );
                    break;
            }
        }

        /// <summary>
        /// Handles the RunEventNotify event from the acquisition monitor.
        /// Raises the SampleComplete event when a sample acquisition is complete.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Run event arguments.</param>
        private void AcquisitionMonitorOnRunEventNotify(object sender, RunEventEventArgs e)
        {
            var runEvent = e.RunEvent;
            var msg = $"[AcquisitionMonitorOnRunEventNotify] - {runEvent.RunEvent}";

            switch (runEvent.RunEvent)
            {
                //case RunEvent.SampleStart:
                case RunEvent.SampleComplete:
                    var sample = ShowSampleStatus(
                        e.InstrumentName,
                        runEvent.SequenceId,
                        runEvent.SampleId,
                        msg
                    );
                    // fire event to observer
                    SampleComplete.RaiseEvent(e, sample, _logger);
                    break;

                case RunEvent.SequenceStart:
                case RunEvent.SequenceComplete:
                    var seq = _acquisitionService.GetSequence(
                        e.InstrumentName,
                        runEvent.SequenceId
                    );
                    ShowSamplesStatus(seq, msg);
                    break;
                default:
                    _logger?.LogInformation(
                        $"[AcquisitionMonitorOnRunEventNotify] - {runEvent.RunEvent}"
                    );
                    break;
            }
        }

        /// <summary>
        /// Retrieves and logs the status of a specific sample.
        /// </summary>
        /// <param name="instrumentName">Instrument name.</param>
        /// <param name="seqId">Sequence ID.</param>
        /// <param name="sampleId">Sample ID.</param>
        /// <param name="msg">Message to log.</param>
        /// <returns>The acquisition sample.</returns>
        private AcquisitionSample ShowSampleStatus(
            string instrumentName,
            Guid seqId,
            Guid sampleId,
            string msg
        )
        {
            var sample = _acquisitionService.GetSample(instrumentName, seqId, sampleId);
            _logger?.LogInformation(
                $"{msg}, Seq ID: {seqId}, Sample ID: {sample.SampleId}, Status: {sample.Status}, RawFileName: {sample.RawFileNameFull}"
            );
            return sample;
        }

        /// <summary>
        /// Logs the status of all samples in a sequence.
        /// </summary>
        /// <param name="acqSeq">The acquisition sequence.</param>
        /// <param name="msg">Message to log.</param>
        private void ShowSamplesStatus(AcquisitionSequence acqSeq, string msg)
        {
            var samples = acqSeq.AcquisitionSamples;
            _logger?.LogInformation($"{msg}, Seq ID: {acqSeq.Id}, Seq Name: {acqSeq.Name}");

            foreach (var sample in samples)
            {
                _logger?.LogInformation($"Sample ID: {sample.SampleId}, Status: {sample.Status}");
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the AcqMonitor and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _isDisposed = true;

                Disconnect();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
