using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ThermoFisher.EventRouterModule.Contracts;

namespace ThermoFisher.EventRouterModule;

/// <summary>
/// Endpoints for the event reouter Module.
/// </summary>
public static class EventRouterModuleEndpoints
{
    /// <summary>
    /// Create a Server-Sent Events (SSE) stream for the specified session Id.
    /// </summary>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="httpContext">Http Context.</param>
    /// <param name="routerService">Ses Service.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Task.</returns>
    [EndpointName("GetEventStream")]
    [EndpointSummary("Get Sse Event Stream")]
    [EndpointDescription("Get Sse Event Stream.")]
    [Tags("SSE Management")]
    public static async Task GetEventStream(
        [FromQuery] [Required] string sessionId, // This is temporary. This will go away when auth is in place
        HttpContext httpContext,
        IEventRouterService routerService,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsync("sessionId is required", cancellationToken);
            return;
        }

        httpContext.Response.ContentType = "text/event-stream";
        await httpContext.Response.Body.FlushAsync(cancellationToken);

        Channel<EventRouterMessage> channel = Channel.CreateUnbounded<EventRouterMessage>();
        await routerService.AddChannel(channel, sessionId, [], cancellationToken);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for a message from the channel
                EventRouterMessage message = await channel.Reader.ReadAsync(cancellationToken);
                var data = System.Text.Json.JsonSerializer.Serialize(message.Data);

                // Write the message to the response stream
                await httpContext.Response.WriteAsync(
                    $"event: {message.Topic}\ndata: {data}\n\n",
                    cancellationToken
                );
                await httpContext.Response.Body.FlushAsync(cancellationToken);
            }
        }
        finally
        {
            await routerService.RemoveChannel(channel, sessionId);
        }
    }

    /// <summary>
    /// Add patterns to the Sse Event Stream.
    /// </summary>
    /// <param name="patterns">Patterns to add.
    /// <remarks>
    /// <para>
    /// Pattern must be a list of words, delimited by dots.
    /// Each word can be one of the following
    /// <list type="bullet">
    /// <item><description>Any empty or non-empty string without *, # and .</description></item>
    /// <item><description>Wildcard *. * (star) can substitute for exactly one word.</description></item>
    /// <item><description>Wildcard #. # (hash) can substitute for zero or more words.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// </param>
    /// <param name="routerService">Sse Service.</param>
    /// <returns>Task.</returns>
    [EndpointName("AddEventStreamPatterns")]
    [EndpointSummary("Add patterns to the Sse Event Stream")]
    [EndpointDescription("Add patterns to the Sse Event Stream.")]
    [Tags("SSE Management")]
    public static async Task PostEventStreamPatterns(
        [FromBody] EventRouterPatterns patterns,
        [FromServices] IEventRouterService routerService
    )
    {
        await routerService.AddPatterns(patterns.SessionId, [.. patterns.Patterns]);
    }

    /// <summary>
    /// Remove patterns from the Sse Event Stream.
    /// <remarks>
    /// <param name="patterns">Patterns to remove.
    /// <remarks>
    /// <para>
    /// Pattern must be a list of words, delimited by dots.
    /// Each word can be one of the following
    /// <list type="bullet">
    /// <item><description>Any empty or non-empty string without *, # and .</description></item>
    /// <item><description>Wildcard *. * (star) can substitute for exactly one word.</description></item>
    /// <item><description>Wildcard #. # (hash) can substitute for zero or more words.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// </param>
    /// <param name="routerService">Sse Service.</param>
    /// <returns>Task.</returns>
    [EndpointName("DeleteEventStreamPatterns")]
    [EndpointSummary("Remove patterns from the Sse Event Stream")]
    [EndpointDescription("Remove patterns from the Sse Event Stream.")]
    [Tags("SSE Management")]
    public static async Task DeleteEventStreamPatterns(
        [FromBody] EventRouterPatterns patterns,
        [FromServices] IEventRouterService routerService
    )
    {
        await routerService.RemovePatterns(patterns.SessionId, [.. patterns.Patterns]);
    }
}
