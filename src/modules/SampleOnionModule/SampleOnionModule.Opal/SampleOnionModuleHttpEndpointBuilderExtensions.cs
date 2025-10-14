using Microsoft.AspNetCore.Builder;
using ThermoFisher.Opal.Shared.Registration.Abstractions;
using ThermoFisher.SampleOnionModule.AspNetCore;

namespace ThermoFisher.SampleOnionModule.Opal;

internal static class SampleOnionModuleHttpEndpointBuilderExtensions
{
    private const string SampleV1Route = "v1/sample";

    public const string OpenApiDocumentName = "sample-onion-v1";

    public static IEndpointConventionBuilder MapSampleOnionModuleEndpoints(
        this IHttpEndpointBuilder endpoints
    )
    {
        var resourceGroup = endpoints
            .MapGroup($"{SampleV1Route}")
            .WithGroupName(OpenApiDocumentName);

        // Test Messaging endpoint
        resourceGroup.MapPost("/sse/topic", SampleOnionModuleEndpoints.TestSSE);
        resourceGroup.MapGet("/message", SampleOnionModuleEndpoints.TestMessaging);

        // Get sample by id endpoint
        resourceGroup.MapGet("/{id}", SampleOnionModuleEndpoints.GetSampleByIdV1Async);
        resourceGroup.MapPost("", SampleOnionModuleEndpoints.CreateSamplesV1Async);

        return resourceGroup;
    }
}
