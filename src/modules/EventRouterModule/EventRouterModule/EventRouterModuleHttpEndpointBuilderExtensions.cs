using Microsoft.AspNetCore.Builder;
using ThermoFisher.Opal.Shared.Registration.Abstractions;

namespace ThermoFisher.EventRouterModule;

/// <summary>
/// Extension methods for registering event router module HTTP endpoints.
/// </summary>
internal static class EventRouterModuleHttpEndpointBuilderExtensions
{
    private const string EventRouterV1Route = "v1";

    /// <summary>
    /// The name of the OpenAPI document for the event router module.
    /// </summary>
    public const string OpenApiDocumentName = "event-router-v1";

    /// <summary>
    /// Maps the event router module endpoints to the specified endpoint builder.
    /// </summary>
    /// <param name="endpoints">Endpoints.</param>
    /// <returns>Builder.</returns>
    public static IEndpointConventionBuilder MapEventRouterModuleEndpoints(
        this IHttpEndpointBuilder endpoints
    )
    {
        var resourceGroup = endpoints
            .MapGroup($"{EventRouterV1Route}")
            .WithGroupName(OpenApiDocumentName);

        resourceGroup.MapGet("/events", EventRouterModuleEndpoints.GetEventStream);
        resourceGroup.MapPost(
            "/events/patterns",
            EventRouterModuleEndpoints.PostEventStreamPatterns
        );
        resourceGroup.MapDelete(
            "/events/patterns",
            EventRouterModuleEndpoints.DeleteEventStreamPatterns
        );

        return resourceGroup;
    }
}
