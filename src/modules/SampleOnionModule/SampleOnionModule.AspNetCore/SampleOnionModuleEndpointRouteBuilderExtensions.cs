using ThermoFisher.SampleOnionModule.AspNetCore;

namespace ThermoFisher.SampleOnionModule.Registration;

internal static class SampleOnionModuleEndpointRouteBuilderExtensions
{
    private const string SampleV1Route = "v1/sample";

    public static IEndpointConventionBuilder MapSampleOnionModuleEndpoints(
        this IEndpointRouteBuilder endpoints
    )
    {
        endpoints.MapGet(
            $"{SampleV1Route}/{{id}}",
            SampleOnionModuleEndpoints.GetSampleByIdV1Async
        );
        return endpoints.MapGet("", SampleOnionModuleEndpoints.CreateSamplesV1Async);
    }
}
