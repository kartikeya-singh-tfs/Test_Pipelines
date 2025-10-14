using Microsoft.AspNetCore.Builder;
using ThermoFisher.AcquisitionModule.AspNetCore;
using ThermoFisher.Opal.Shared.Registration.Abstractions;

namespace ThermoFisher.AcquisitionModule.Opal;

internal static class AcquisitionModuleHttpEndpointBuilderExtensions
{
    private const string AcquisitionV1Route = "v1";

    public const string OpenApiDocumentName = "acquisition-v1";

    public static IEndpointConventionBuilder MapSampleOnionModuleEndpoints(
        this IHttpEndpointBuilder endpoints
    )
    {
        var resourceGroup = endpoints
            .MapGroup($"{AcquisitionV1Route}")
            .WithGroupName(OpenApiDocumentName);

        // Get sample by id endpoint
        resourceGroup.MapPost("/sequence", AcquisitionModuleEndpoints.SubmitSequenceAsync);
        resourceGroup.MapGet("/sequence", AcquisitionModuleEndpoints.GetSequenceAsync);
        resourceGroup.MapGet("/sse", AcquisitionModuleEndpoints.GetSse);
        resourceGroup.MapGet("/sseSparklineData", AcquisitionModuleEndpoints.GetSseSparklineData);
        resourceGroup.MapGet(
            "/sseChromatogramSvgData",
            AcquisitionModuleEndpoints.GetSseChromatogramSvgData
        );

        return resourceGroup;
    }
}
