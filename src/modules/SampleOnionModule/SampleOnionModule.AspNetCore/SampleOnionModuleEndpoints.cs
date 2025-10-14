using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using SampleOnionModule.Contracts;
using ThermoFisher.EventRouterModule.Contracts;
using ThermoFisher.SampleOnionModule.Abstractions;
using ThermoFisher.SampleOnionModule.Contracts;

namespace ThermoFisher.SampleOnionModule.AspNetCore;

public class SampleOnionModuleEndpoints
{
    [Produces(typeof(SampleResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointName("GetSampleById")]
    [EndpointSummary("Get Sample by ID")]
    [EndpointDescription("Retrieves a sample by its unique identifier.")]
    [Tags(["Sample Management"])]
    public static async Task<IResult> GetSampleByIdV1Async(
        [FromRoute] [Description("Sample identifier")] string id,
        [FromServices] ISampleOnionModule sampleOnionModule,
        CancellationToken cancellationToken = default
    )
    {
        var decodedId = Uri.UnescapeDataString(id);

        // Simulate fetching sample data
        var sample = await sampleOnionModule.GetSampleByIdAsync(decodedId, cancellationToken);
        var response = new SampleResponse(
            sample.Id.ToString(),
            sample.Name,
            sample.Description,
            sample.CreatedAt
        );

        return TypedResults.Ok(response);
    }

    [ProducesResponseType(StatusCodes.Status201Created)]
    [EndpointName("CreateSamples")]
    [EndpointSummary("Create one or more samples")]
    [EndpointDescription(
        """
              Creates one or more samples.
            """
    )]
    [Tags(["Sample Management"])]
    public static async Task<IResult> CreateSamplesV1Async(
        [FromBody] [Description("Request parameters")] CreateSamplesRequest request,
        [FromServices] ISampleOnionModule sampleOnionModule,
        CancellationToken cancellationToken = default
    )
    {
        await sampleOnionModule.CreateSamplesAsync(
            request.Count,
            request.NamePrefix,
            request.Description,
            cancellationToken
        );

        return TypedResults.Created();
    }

    [EndpointName("TestSSE")]
    [EndpointSummary("Tests SSE")]
    [EndpointDescription("Tests SSE messaging.")]
    [Tags(["Messaging Example"])]
    public static async Task TestSSE(
        [FromQuery] string topic,
        [FromQuery] string message,
        [FromServices] ITestMessagingService messenger,
        CancellationToken cancellationToken = default
    )
    {
        await messenger.PublishEventRouterMessageAsync(new EventRouterMessage(topic, message));
    }

    [Produces(typeof(AddResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointName("TestMessaging")]
    [EndpointSummary("Tests messaging")]
    [EndpointDescription("Tests all messaging scenarios.")]
    [Tags(["Messaging Example"])]
    public static async Task<IResult> TestMessaging(
        [FromServices] ITestMessagingService messenger,
        [FromServices] ILogger<SampleOnionModuleEndpoints> logger,
        CancellationToken cancellationToken = default
    )
    {
        await messenger.TestAsync();
        var sum = await messenger.AddAsync(5, 10, cancellationToken);
        return TypedResults.Ok(sum);
    }
}
