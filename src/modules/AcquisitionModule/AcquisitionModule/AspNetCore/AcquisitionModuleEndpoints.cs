using System.ComponentModel;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ThermoFisher.AcquisitionModule.Abstractions;
using ThermoFisher.AcquisitionModule.Contracts;
using ThermoFisher.AcquisitionModule.Grpc;

namespace ThermoFisher.AcquisitionModule.AspNetCore;

public static class AcquisitionModuleEndpoints
{
    //[Produces(typeof(SampleResponse))]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointName("SubmitSequence")]
    [EndpointSummary("Submit sequence")]
    [EndpointDescription("Submit sequence.")]
    [Tags(["Acquisition Management"])]
    public static async Task<IResult> SubmitSequenceAsync(
        [FromBody] [Description("Sequence")] SequenceData sequence,
        [FromServices] AcquisitionService acq,
        CancellationToken cancellationToken = default
    )
    {
        await acq.SubmitSequenceAsync(sequence);
        return TypedResults.Ok(new { Message = "Sequence submitted successfully." });
    }

    [EndpointName("GetSequences")]
    [EndpointSummary("Get sequences")]
    [EndpointDescription("Get sequences.")]
    [Tags(["Acquisition Management"])]
    public static IEnumerable<Sequence> GetSequenceAsync(AcquisitionService acq)
    {
        return acq.GetSequences();
    }

    [EndpointName("GetSSE")]
    [EndpointSummary("Get sequence status using SSE")]
    [EndpointDescription("Get sequence status using SSE.")]
    [Tags(["Acquisition Management"])]
    public static async Task GetSse(
        AcquisitionService acq,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        httpContext.Response.ContentType = "text/event-stream";
        await httpContext.Response.Body.FlushAsync(cancellationToken);

        Channel<string> channel = Channel.CreateUnbounded<string>();
        acq.AddSseChannel(channel);

        while (!cancellationToken.IsCancellationRequested)
        {
            // Wait for a message from the channel
            string message = await channel.Reader.ReadAsync(cancellationToken);
            // Write the message to the response stream
            await httpContext.Response.WriteAsync($"data: {message}\n\n", cancellationToken);
            await httpContext.Response.Body.FlushAsync(cancellationToken);
        }

        acq.RemoveSseChannel(channel);
    }

    [EndpointName("GetSseSparklineData")]
    [EndpointSummary("Get sparkline data using SSE")]
    [EndpointDescription("Get sparkline data using SSE.")]
    [Tags(["Acquisition Management"])]
    public static async Task GetSseSparklineData(
        RealTimePlotDataAccessService rda,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        httpContext.Response.ContentType = "text/event-stream";
        await httpContext.Response.Body.FlushAsync(cancellationToken);

        var channel = Channel.CreateUnbounded<string>();
        var typeName = typeof(SparklineDataDto).ToString();
        rda.AddSseChannel(typeName, channel);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for a message from the channel
                var message = await channel.Reader.ReadAsync(cancellationToken);
                // Write the message to the response stream
                await httpContext.Response.WriteAsync($"data: {message}\n\n", cancellationToken);
                await httpContext.Response.Body.FlushAsync(cancellationToken);
            }
        }
        finally
        {
            rda.RemoveSseChannel(typeName, channel);
        }
    }

    [EndpointName("GetSseChromatogramSvgData")]
    [EndpointSummary("Get chromatogram SVG using SSE")]
    [EndpointDescription("Get chromatogram SVG using SSE.")]
    [Tags(["Acquisition Management"])]
    public static async Task GetSseChromatogramSvgData(
        RealTimePlotDataAccessService rda,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        httpContext.Response.ContentType = "text/event-stream";
        await httpContext.Response.Body.FlushAsync(cancellationToken);

        var channel = Channel.CreateUnbounded<string>();
        var typeName = typeof(ChromatogramSvgDataDto).ToString();
        rda.AddSseChannel(typeName, channel);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for a message from the channel
                var message = await channel.Reader.ReadAsync(cancellationToken);

                // Write the message to the response stream
                await httpContext.Response.WriteAsync($"data: {message}\n\n", cancellationToken);
                await httpContext.Response.Body.FlushAsync(cancellationToken);
            }
        }
        finally
        {
            rda.RemoveSseChannel(typeName, channel);
        }
    }
}
