namespace ThermoFisher.SampleVerticalModule.Chromatography.Abstractions;

public interface IChromatogramAnalysis
{
    Task<ChromatogramAnalysis> AnalyzeAsync(
        string sampleId,
        string methodName,
        CancellationToken cancellationToken = default
    );
}
