using Microsoft.AspNetCore.Builder;
using ThermoFisher.Opal.Shared.Registration.Abstractions;
using ThermoFisher.SampleVerticalModule.MassSpectrometry;

namespace ThermoFisher.SampleVerticalModule.Opal;

internal static class MassSpecHttpEndpointBuilderExtensions
{
    public const string OpenApiDocumentName = "mass-spec-v1";
    private const string MassSpecAnalysisV1Route = "v1/mass-spec-analysis";

    public static IEndpointConventionBuilder MapMassSpecEndpoints(
        this IHttpEndpointBuilder endpoints
    )
    {
        var group = endpoints.MapGroup(MassSpecAnalysisV1Route).WithGroupName(OpenApiDocumentName);

        group.MapPost("/", MassSpecAnalysisEndpoints.AnalyzeMassSpecAsync);

        return group;
    }
}
