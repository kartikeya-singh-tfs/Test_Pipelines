using Microsoft.AspNetCore.Builder;
using ThermoFisher.Opal.Shared.Registration.Abstractions;
using ThermoFisher.SampleVerticalModule.Chromatography;

namespace ThermoFisher.SampleVerticalModule.Opal;

internal static class ChromatographyHttpEndpointBuilderExtensions
{
    private const string ChromatogramAnalysisV1Route = "v1/chromatogram-analysis";

    public const string OpenApiDocumentName = "chromatogram-analysis-v1";

    public static IEndpointConventionBuilder MapChromatographyEndpoints(
        this IHttpEndpointBuilder endpoints
    )
    {
        var group = endpoints
            .MapGroup(ChromatogramAnalysisV1Route)
            .WithGroupName(OpenApiDocumentName);

        group.MapPost("/", ChromatographyEndpoints.AnalyzeChromatogramAsync);

        return group;
    }
}
