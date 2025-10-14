using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ThermoFisher.AcquisitionModule.Abstractions;
using ThermoFisher.AcquisitionModule.Contracts;

namespace ThermoFisher.AcquisitionModule.Grpc
{
    public class AcquisitionService : Acquisition.AcquisitionBase
    {
        private readonly ILogger<AcquisitionService> _logger;
        private readonly ConcurrentDictionary<Guid, Sequence> _sequences = new();
        private Channel<SequenceReplyStream> _sequenceChannel =
            Channel.CreateUnbounded<SequenceReplyStream>();
        private ConcurrentDictionary<Channel<string>, Channel<string>> _sseChannels =
            new ConcurrentDictionary<Channel<string>, Channel<string>>();

        public AcquisitionService(ILogger<AcquisitionService> logger)
        {
            _logger = logger;
        }

        public void AddSseChannel(Channel<string> channel)
        {
            _sseChannels.TryAdd(channel, channel);
        }

        public void RemoveSseChannel(Channel<string> channel)
        {
            _sseChannels.TryRemove(channel, out _);
        }

        public IEnumerable<Sequence> GetSequences()
        {
            return _sequences.Values;
        }

        public async Task SubmitSequenceAsync(SequenceData sequence)
        {
            if (_sequences.ContainsKey(sequence.Id))
            {
                // TODO
                // throw new InvalidOperationException($"Sequence with ID {sequence.Id} already exists.");
            }

            _sequences[sequence.Id] = new Sequence
            {
                Id = sequence.Id,
                Name = sequence.Name,
                Description = sequence.Description,
                Samples = sequence
                    .Samples.Select(s => new Sample
                    {
                        SampleData = new SampleData
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Type = s.Type,
                            MethodFilePath = s.MethodFilePath,
                            RawFilePath = s.RawFilePath,
                            Position = s.Position,
                            Volume = s.Volume,
                        },
                    })
                    .ToList(),
            };

            var seq = new SequenceReplyStream { Id = sequence.Id.ToString() };

            sequence.Samples.ForEach(sample =>
            {
                seq.Samples.Add(
                    new SampleReply
                    {
                        Id = sample.Id.ToString(),
                        SampleName = sample.Name,
                        MethodFilePath = sample.MethodFilePath,
                        RawFilePath = sample.RawFilePath,
                    }
                );
            });

            await _sequenceChannel.Writer.WriteAsync(seq);
        }

        public override async Task SubmitSampleStream(
            EmptyRequest request,
            IServerStreamWriter<SequenceReplyStream> responseStream,
            ServerCallContext context
        )
        {
            try
            {
                var seq = await _sequenceChannel.Reader.ReadAsync();
                await responseStream.WriteAsync(seq);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SubmitSampleStream");
            }
        }

        public override Task<EmptyResponse> SequenceStateEvent(
            SequenceEvent request,
            ServerCallContext context
        )
        {
            _logger.LogInformation(
                "SequenceStateEvent Received: Sequence State: {SequenceState} for Sequence ID: {SequenceId}",
                request.SequenceState,
                request.SequenceId
            );

            var sequenceStatus = new SequenceStatus
            {
                SequenceId = request.SequenceId,
                SampleId = request.SampleId,
                SequenceState = (Abstractions.SequenceState)request.SequenceState,
                IsError = request.IsError,
            };

            if (_sequences.TryGetValue(Guid.Parse(request.SequenceId), out var sequence))
            {
                sequence.SequenceStatus = sequenceStatus;
            }

            if (
                request.SequenceState
                == ThermoFisher
                    .AcquisitionModule
                    .Contracts
                    .SequenceState
                    .SequenceCompletedFilesMoved
            )
            {
                // TODO
                // _sequences.Remove(Guid.Parse(request.SequenceId), out _);
            }

            SendDataToChannel(sequenceStatus);
            return Task.FromResult(new EmptyResponse());
        }

        public override Task<EmptyResponse> AcquisitionStateEvent(
            AcquisitionEvent request,
            ServerCallContext context
        )
        {
            _logger.LogInformation(
                "AcquisitionStateEvent Received: Acquisition State: {AcquisitionState} for Sequence ID: {SequenceId}, Sample ID: {SampleId}",
                request.AcquisitionState,
                request.SequenceId,
                request.SampleId
            );

            var sampleStatus = new SampleStatus
            {
                SequenceId = request.SequenceId,
                SampleId = request.SampleId,
                AcquisitionState = (Abstractions.AcquisitionState)request.AcquisitionState,
            };

            if (_sequences.TryGetValue(Guid.Parse(request.SequenceId), out var sequence))
            {
                var sample = sequence.Samples.Find(s =>
                    s.SampleData.Id.ToString() == request.SampleId
                );
                if (sample != null)
                {
                    sample.SampleStatus = sampleStatus;
                }
            }

            SendDataToChannel(sampleStatus);
            return Task.FromResult(new EmptyResponse());
        }

        public override Task<EmptyResponse> DeviceStateEvent(
            DeviceEvent request,
            ServerCallContext context
        )
        {
            _logger.LogInformation(
                "DeviceStateEvent Received: Device Status Count: {DeviceStatusCount}",
                request.DeviceStatus.Count
            );
            SendDataToChannel(request);
            return Task.FromResult(new EmptyResponse());
        }

        private void SendDataToChannel<T>(T data)
        {
            var obj = new { Type = data?.GetType().Name, Data = data };
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
            };
            var str = JsonSerializer.Serialize(obj, options);
            foreach (var item in _sseChannels)
            {
                item.Key.Writer.WriteAsync(str);
            }
        }
    }
}
