using ThermoFisher.SampleVerticalModule.Chromatography.Abstractions;

namespace ThermoFisher.SampleVerticalModule.Chromatography;

public static class ChromatographyServiceCollectionExtensions
{
    public static IServiceCollection AddChromatography(this IServiceCollection services)
    {
        services.AddScoped<IChromatogramAnalysis, ChromatogramAnalysisService>();

        return services;
    }
}
