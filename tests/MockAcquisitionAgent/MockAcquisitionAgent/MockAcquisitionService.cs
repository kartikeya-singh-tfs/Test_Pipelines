
using Grpc.Core;
using System.Threading.Tasks;
using ThermoFisher.AcquisitionModule.Contracts;
using System.Collections.Concurrent;

// Mock service for plot data uploads (sparkline + chromatogram)
public class MockAcquisitionService : RealTimePlotService.RealTimePlotServiceBase
{
    public override Task<PlotDataResponse> UploadSparklineData(SparklineData request, ServerCallContext context)
    {
        var response = new PlotDataResponse
        {
            Status = UploadStatusType.Ok,
            Message = $"Mock upload sparkline successful scans {request.StartScanNumber}-{request.EndScanNumber} count={request.Intensities.Count}"
        };
        return Task.FromResult(response);
    }

    public override Task<PlotDataResponse> UploadChromatogramSvgData(ChromatogramSvgData request, ServerCallContext context)
    {
        var response = new PlotDataResponse
        {
            Status = UploadStatusType.Ok,
            Message = $"Mock chromatogram upload successful scans {request.StartScanNumber}-{request.EndScanNumber}"
        };
        return Task.FromResult(response);
    }
}

// Additional mock lifecycle service to emulate acquisition state RPCs used in GrpcCallReplicationTest
public class MockAcquisitionLifecycleService : Acquisition.AcquisitionBase
{
    private readonly ConcurrentQueue<string> _log = new();

    public override Task<EmptyResponse> SequenceStateEvent(SequenceEvent request, ServerCallContext context)
    {
        _log.Enqueue($"SequenceStateEvent: seq={request.SequenceId} sample={request.SampleId} state={request.SequenceState}");
        return Task.FromResult(new EmptyResponse());
    }

    public override Task<EmptyResponse> AcquisitionStateEvent(AcquisitionEvent request, ServerCallContext context)
    {
        _log.Enqueue($"AcquisitionStateEvent: seq={request.SequenceId} sample={request.SampleId} acqState={request.AcquisitionState}");
        return Task.FromResult(new EmptyResponse());
    }

    public override Task<EmptyResponse> DeviceStateEvent(DeviceEvent request, ServerCallContext context)
    {
        _log.Enqueue($"DeviceStateEvent: statuses={request.DeviceStatus.Count}");
        return Task.FromResult(new EmptyResponse());
    }

    public override async Task SubmitSampleStream(EmptyRequest request, IServerStreamWriter<SequenceReplyStream> responseStream, ServerCallContext context)
    {
        // Provide a minimal mock sequence reply to satisfy streaming client expectations.
        var reply = new SequenceReplyStream { Id = "mock-sequence" };
        reply.Samples.Add(new SampleReply { Id = "mock-sample", SampleName = "MockSample", MethodFilePath = "method.meth", RawFilePath = "sample.raw" });
        await responseStream.WriteAsync(reply);
    }

    // Helper to drain logs if needed in future extensions
    public string[] DrainLogs()
    {
        var list = new System.Collections.Generic.List<string>();
        while (_log.TryDequeue(out var entry)) list.Add(entry);
        return list.ToArray();
    }
}
