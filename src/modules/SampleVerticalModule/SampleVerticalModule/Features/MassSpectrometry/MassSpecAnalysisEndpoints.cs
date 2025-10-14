using Microsoft.AspNetCore.Mvc;
using ThermoFisher.SampleVerticalModule.MassSpectrometry.Abstractions;
using ThermoFisher.SampleVerticalModule.MassSpectrometry.Contracts;

namespace ThermoFisher.SampleVerticalModule.MassSpectrometry;

public static class MassSpecAnalysisEndpoints
{
    [EndpointName("AnalyzeMassSpec")]
    [EndpointSummary("Analyze Mass Spectrometry Data")]
    [EndpointDescription("Performs mass spectrometry analysis for a given sample.")]
    [Tags("Mass Spectrometry Analysis")]
    [ProducesResponseType(typeof(MassSpecAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<IResult> AnalyzeMassSpecAsync(
        [FromBody] MassSpecAnalysisRequest request,
        [FromServices] IMassSpectrometryAnalysis service
    )
    {
        if (string.IsNullOrEmpty(request.SampleId))
        {
            return Results.BadRequest("Sample ID is required");
        }

        var result = await service.AnalyzeAsync(request.SampleId, request.IonizationMode);
        var response = new MassSpecAnalysisResponse(
            SampleId: result.SampleId,
            IonizationMode: result.IonizationMode,
            MassRange: result.MassRange,
            SpectraCount: result.SpectraCount,
            BasePeakIntensity: result.BasePeakIntensity,
            AnalyzedAt: DateTime.UtcNow
        );

        return TypedResults.Ok(response);
    }
}
