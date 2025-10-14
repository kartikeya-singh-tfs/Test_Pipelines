using ThermoFisher.SampleVerticalModule.Chromatography.Abstractions;

namespace ThermoFisher.SampleVerticalModule.Chromatography;

internal class ChromatogramAnalysisService : IChromatogramAnalysis
{
    public async Task<ChromatogramAnalysis> AnalyzeAsync(
        string sampleId,
        string methodName,
        CancellationToken cancellationToken = default
    )
    {
        await Task.Delay(200);

        var response = new ChromatogramAnalysis(
            sampleId,
            methodName,
            Random.Shared.Next(10, 100),
            Random.Shared.NextDouble() * 30 + 5,
            "Completed",
            DateTime.UtcNow
        );

        return response;
    }
}
