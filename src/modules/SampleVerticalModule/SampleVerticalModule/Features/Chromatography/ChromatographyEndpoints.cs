using Microsoft.AspNetCore.Mvc;
using ThermoFisher.SampleVerticalModule.Chromatography.Abstractions;
using ThermoFisher.SampleVerticalModule.Chromatography.Contracts;

namespace ThermoFisher.SampleVerticalModule.Chromatography;

public static class ChromatographyEndpoints
{
    [EndpointName("AnalyzeChromatogram")]
    [EndpointSummary("Analyze Chromatogram")]
    [EndpointDescription("Performs chromatogram analysis for a given sample.")]
    [Tags("Chromatogram Analysis")]
    [ProducesResponseType(typeof(ChromatogramAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<IResult> AnalyzeChromatogramAsync(
        [FromBody] ChromatogramAnalysisRequest request,
        [FromServices] IChromatogramAnalysis service
    )
    {
        if (string.IsNullOrEmpty(request.SampleId))
        {
            return Results.BadRequest("Sample ID is required");
        }

        var result = await service.AnalyzeAsync(request.SampleId, request.MethodName);
        var response = new ChromatogramAnalysisResponse(
            SampleId: result.SampleId,
            MethodName: result.MethodName,
            PeakCount: result.PeakCount,
            RetentionTimeRange: result.RetentionTimeRange,
            AnalyzedAt: DateTime.UtcNow
        );

        return TypedResults.Ok(response);
    }
}
