using Grpc.Core;
using ThermoFisher.SampleOnionModule.Abstractions;
using ThermoFisher.SampleOnionModule.Contracts;

namespace ThermoFisher.SampleOnionModule.Grpc;

public class SampleOnionGrpcService : SampleOnionService.SampleOnionServiceBase
{
    private readonly ISampleOnionModule _sampleOnionModuleService;

    public SampleOnionGrpcService(ISampleOnionModule sampleOnionModuleService)
    {
        _sampleOnionModuleService = sampleOnionModuleService;
    }

    public override async Task<GetCompoundsResponse> GetCompounds(
        GetCompoundsRequest request,
        ServerCallContext context
    )
    {
        var result = new GetCompoundsResponse();
        await foreach (
            var compound in _sampleOnionModuleService.GetCompoundsAsync(context.CancellationToken)
        )
        {
            result.Compounds.Add(
                new Contracts.Compound
                {
                    Name = compound.Name,
                    Formula = compound.Formula,
                    PeakArea = compound.PeakArea,
                    RetentionTime = compound.RetentionTime,
                    Concentration = compound.Concentration,
                }
            );
        }

        return result;
    }
}
