using ThermoFisher.SampleVerticalModule.MassSpectrometry.Abstractions;

namespace ThermoFisher.SampleVerticalModule.MassSpectrometry;

public static class MassSpecServiceCollectionExtensions
{
    public static IServiceCollection AddMassSpectrometry(this IServiceCollection services)
    {
        services.AddScoped<IMassSpectrometryAnalysis, MassSpecAnalysisService>();

        return services;
    }
}
