using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ThermoFisher.Foundation.Acquisition;

namespace OnPremAcquisition
{
    public class AcquisitionServiceAccess : IAcquisitionServiceAccess
    {
        private const string LocalHost = "localhost";
        private ILogger _logger;
        private bool _isDisposed = false;
        private bool _isSequenceInQueue = false;

        private AcquisitionMonitor _acqMonitor;
        private IAcquisition _acqService;

        public DeviceStatusInfo[] Devices { get; private set; }

        public event Action<RunEventEventArgs> RunEventNotify = delegate { };
        public event Action<DeviceStatusInfo[]> DeviceStatusChanged = delegate { };
        public event Action<RunStateEventArgs> RunStateChanged = delegate { };
        public event Action<RunEventEventArgs, AcquisitionSample> SampleComplete = delegate { };

        public AcquisitionServiceAccess(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AcquisitionServiceAccess>();
            Task.Run(() => WaitForAcquisitionServiceToStop()).ConfigureAwait(false);
            ConnectWithNewMonitor(false);
        }

        private void WaitForAcquisitionServiceToStop()
        {
            ServiceController sc = new ServiceController(
                "ThermoFisher.Foundation.AcquisitionService"
            );

            while (true)
            {
                sc.WaitForStatus(ServiceControllerStatus.Running);
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
                ProcessExited(null, null);
            }
        }

        private void ConnectWithNewMonitor(bool connectAsync)
        {
            CleanUp();

            _acqMonitor = new AcquisitionMonitor();

            if (!ConnectToAcquisitionService())
                return;

            if (connectAsync)
            {
                Task.Run(() =>
                {
                    while (!ConnectToServer())
                    {
                        Thread.Sleep(1000);
                    }
                });
            }
            else
            {
                if (!ConnectToServer())
                {
                    return;
                }
            }
        }

        private void CleanUp()
        {
            if (_acqMonitor != null)
            {
                try
                {
                    _acqMonitor.RunEventNotify -= OnRunEventNotify;
                    _acqMonitor.DeviceStatusChanged -= OnDeviceStatusChanged;
                    _acqMonitor.SequenceListChanged -= OnSequenceListChanged;
                    _acqMonitor.RunStateChanged -= OnRunStateChanged;
                }
                catch
                {
                    // do nothing
                }
            }
        }

        private bool ConnectToAcquisitionService()
        {
            try
            {
                _acqService = AcquisitionService.GetServer();
                _logger.LogInformation(
                    "AcquisitionServiceAccess.ConnectToAcquisitionService connected to acquisition server"
                );
                return (null != _acqService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AcquisitionServiceAccess.ConnectToAcquisitionService error");
            }

            return false;
        }

        private bool ConnectToServer()
        {
            try
            {
                // connect event monitor to acquisition service
                if (_acqMonitor.Connect(_acqService, true))
                {
                    _logger.LogInformation("AcquisitionMonitor connected");
                    OnConnected();
                }
                else
                {
                    _logger.LogInformation("AcquisitionMonitor did not connect");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AcquisitionServiceAccess.ConnectToServer error");
                return false;
            }

            return true;
        }

        private void OnConnected()
        {
            Devices = _acqService.GetDeviceStatus(null);

            _acqMonitor.RunEventNotify += OnRunEventNotify;
            _acqMonitor.DeviceStatusChanged += OnDeviceStatusChanged;
            _acqMonitor.SequenceListChanged += OnSequenceListChanged;
            _acqMonitor.RunStateChanged += OnRunStateChanged;

            OnSequenceListChanged(null, null);

            foreach (var device in Devices)
            {
                _logger.LogInformation(
                    $"Device : {device.Name} ({device.DeviceType}) - Status: {device.Status}"
                );
            }
        }

        void ProcessExited(object sender, EventArgs e)
        {
            _logger.LogInformation("Acquisition Service Process Exited.");

            try
            {
                _acqMonitor.Disconnect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Acquisition service monitor disconnect failed.");

                // do nothing
            }

            ConnectWithNewMonitor(true);
        }

        private void OnRunStateChanged(object sender, RunStateEventArgs e)
        {
            _logger.LogInformation("OnRunStateChanged : " + e.RunState.RunState);
            RunStateChanged(e);
        }

        private void OnDeviceStatusChanged(object sender, DeviceStatusEventArgs e)
        {
            _logger.LogInformation(
                "OnDeviceStatusChanged : "
                    + e.DeviceStatus.DeviceType
                    + " : "
                    + e.DeviceStatus.Status
            );

            Task.Run(() =>
            {
                Devices = _acqService.GetDeviceStatus(null);
                DeviceStatusChanged(Devices);
            });
        }

        private void OnRunEventNotify(object sender, RunEventEventArgs e)
        {
            _logger.LogInformation("OnRunEventNotify : " + e.RunEvent.RunEvent);

            if (e.RunEvent.RunEvent == RunEvent.SampleComplete)
            {
                var sample = _acqService.GetSample(
                    null,
                    e.RunEvent.SequenceId,
                    e.RunEvent.SampleId
                );
                SampleComplete(e, sample);
            }
            else
            {
                RunEventNotify(e);
            }
        }

        private void OnSequenceListChanged(object sender, InstrumentEventArgs e)
        {
            Guid[] seq = _acqService.GetSequenceQueue(null);

            if ((seq != null) && (seq.Length > 0))
            {
                _isSequenceInQueue = true;
            }
            else
            {
                _isSequenceInQueue = false;
            }
        }

        public void Dispose()
        {
            try
            {
                if (!_isDisposed) // if the acquisiiton service is running
                {
                    if (null != _acqMonitor) // if this client has established connection
                    {
                        _acqMonitor.Disconnect();

                        _logger.LogInformation(
                            "AcquisitionServiceAccess: AcquisitionMonitor.Disconnect call at "
                                + DateTime.Now
                        );
                    }

                    AcquisitionService.Disconnect(LocalHost);

                    _logger.LogInformation(
                        "AcquisitionServiceAccess: AcquisitionService.Disconnect call at "
                            + DateTime.Now
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AcquisitionServiceAccess.Dispose");
            }
        }

        public string[] GetDeviceNames()
        {
            List<string> retVal = new List<string>();
            DeviceStatusInfo[] devices = _acqService.GetDeviceStatus(null);

            foreach (var device in devices)
            {
                retVal.Add(device.Name);
            }

            return retVal.ToArray();
        }

        public bool SubmitSequence(
            string instrumentName,
            AcquisitionSequence sequence,
            bool isHighPriority,
            bool enforceUser,
            CfrAuditData auditData
        )
        {
            bool retVal = _acqService.SubmitSequence(
                instrumentName,
                sequence,
                isHighPriority,
                enforceUser,
                auditData
            );
            return retVal;
        }

        public bool InsertSample(
            string instrumentName,
            Guid sequenceId,
            int index,
            AcquisitionSample acquisitionSample
        )
        {
            return _acqService.InsertSample(instrumentName, sequenceId, index, acquisitionSample);
        }

        public bool RemoveSamples(string instrumentName, Guid sequenceId, Guid[] sampleIds)
        {
            return _acqService.RemoveSamples(instrumentName, sequenceId, sampleIds);
        }

        public bool StopRun(string instrumentName)
        {
            return _acqService.StopRun(instrumentName);
        }

        public bool IsQueueEmpty()
        {
            Guid[] sequences = _acqService.GetSequenceQueue(null);
            foreach (var seq in sequences)
            {
                if (
                    _acqService.GetSequenceStatus(null, seq)
                    != ThermoFisher.Foundation.Acquisition.SequenceStatus.Complete
                )
                {
                    return false;
                }
            }

            return true;
        }
    }
}
