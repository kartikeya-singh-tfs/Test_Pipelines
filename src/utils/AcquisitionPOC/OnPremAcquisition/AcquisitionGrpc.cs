using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using ThermoFisher.AcquisitionModule.Contracts;
using ThermoFisher.Foundation.Acquisition;

namespace OnPremAcquisition
{
    public class AcquisitionGrpc : IAcquisitionGrpc
    {
        private ILogger _logger;
        private AcquisitionAdapter _acquisitionWrapper;
        private Channel _channel;
        private Acquisition.AcquisitionClient _client;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public AcquisitionGrpc(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AcquisitionGrpc>();
            _acquisitionWrapper = new AcquisitionAdapter(loggerFactory);

            _acquisitionWrapper.RunEventNotify += OnRunEventNotify;
            _acquisitionWrapper.DeviceStatusChanged += OnDeviceStatusChanged;
            _acquisitionWrapper.RunStateChanged += OnRunStateChanged;
            _acquisitionWrapper.SampleComplete += OnSampleComplete;
        }

        public void Start()
        {
            string grpcServerAddress = System.Configuration.ConfigurationManager.AppSettings[
                "GrpcServer"
            ];
            _channel = new Channel(grpcServerAddress, ChannelCredentials.Insecure);

            _client = new Acquisition.AcquisitionClient(_channel);

            _ = WaitOnSequenceSubmission();
        }

        public IAcquisitionAdapter GetAcquisitionAdapter() => _acquisitionWrapper;

        private async Task WaitOnSequenceSubmission()
        {
            var ct = _cancellationTokenSource.Token;
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using (
                        var call = _client.SubmitSampleStream(new EmptyRequest(), null, null, ct)
                    )
                    {
                        while (await call.ResponseStream.MoveNext(ct))
                        {
                            var reply = call.ResponseStream.Current;
                            _logger.LogInformation("Sequence Received");

                            var sequence = new SequenceData
                            {
                                Id = Guid.Parse(reply.Id),
                                Samples = reply
                                    .Samples.Select(s => new SampleData
                                    {
                                        Id = Guid.Parse(s.Id),
                                        SampleName = s.SampleName,
                                        MethodFilePath = s.MethodFilePath,
                                        RawFilePath = s.RawFilePath,
                                    })
                                    .ToList(),
                            };

                            if (!ct.IsCancellationRequested)
                            {
                                _acquisitionWrapper.SubmitSequence(sequence);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error in sequence submission stream");
                }
            }
        }

        private void OnDeviceStatusChanged(DeviceStatusInfo[] deviceStatus)
        {
            var deviceEvent = new ThermoFisher.AcquisitionModule.Contracts.DeviceEvent();
            foreach (var item in deviceStatus)
            {
                deviceEvent.DeviceStatus.Add(
                    new ThermoFisher.AcquisitionModule.Contracts.DeviceStatus
                    {
                        DeviceName = item.Name,
                        DeviceType = (ThermoFisher.AcquisitionModule.Contracts.DeviceType)
                            item.DeviceType,
                        DeviceState = (ThermoFisher.AcquisitionModule.Contracts.DeviceState)
                            item.Status,
                    }
                );
            }

            _client?.DeviceStateEvent(deviceEvent);
        }

        private void OnRunEventNotify(RunEventEventArgs args)
        {
            if (args.RunEvent.RunEvent == RunEvent.SampleComplete)
            {
                // skip sample complete events
                // these will fired in SampleComplete handler
                return;
            }

            _client?.SequenceStateEvent(
                new SequenceEvent
                {
                    SequenceId = args.RunEvent.SequenceId.ToString(),
                    SampleId = args.RunEvent.SampleId.ToString(),
                    SequenceState = (ThermoFisher.AcquisitionModule.Contracts.SequenceState)
                        args.RunEvent.RunEvent,
                    IsError = args.RunEvent.IsError,
                }
            );
        }

        private void OnRunStateChanged(RunStateEventArgs args)
        {
            _client?.AcquisitionStateEvent(
                new AcquisitionEvent
                {
                    SequenceId = args.RunState.SequenceId.ToString(),
                    SampleId = args.RunState.SampleId.ToString(),
                    AcquisitionState = (ThermoFisher.AcquisitionModule.Contracts.AcquisitionState)
                        args.RunState.RunState,
                }
            );
        }

        private void OnSampleComplete(RunEventEventArgs runEvent, AcquisitionSample sample)
        {
            _client?.SequenceStateEvent(
                new SequenceEvent
                {
                    SequenceId = runEvent.RunEvent.SequenceId.ToString(),
                    SampleId = runEvent.RunEvent.SampleId.ToString(),
                    SequenceState = (ThermoFisher.AcquisitionModule.Contracts.SequenceState)
                        runEvent.RunEvent.RunEvent,
                    IsError = runEvent.RunEvent.IsError,
                }
            );
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _channel?.ShutdownAsync().Wait();
            _acquisitionWrapper?.Dispose();
        }
    }
}
