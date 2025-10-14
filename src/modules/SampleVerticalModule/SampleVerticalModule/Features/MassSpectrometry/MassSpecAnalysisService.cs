using ThermoFisher.SampleVerticalModule.MassSpectrometry.Abstractions;

namespace ThermoFisher.SampleVerticalModule.MassSpectrometry;

internal class MassSpecAnalysisService : IMassSpectrometryAnalysis
{
    public async Task<MassSpecAnalysis> AnalyzeAsync(
        string sampleId,
        string ionizationMode,
        CancellationToken cancellationToken = default
    )
    {
        await Task.Delay(300);

        var massRange = new double[] { 100.0, 1500.0 };
        var response = new MassSpecAnalysis(
            sampleId,
            ionizationMode,
            massRange,
            Random.Shared.Next(1000, 10000),
            Random.Shared.NextDouble() * 1000000 + 50000,
            "Completed",
            DateTime.UtcNow
        );

        return response;
    }
}
