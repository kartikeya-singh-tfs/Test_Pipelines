namespace ThermoFisher.SampleVerticalModule.MassSpectrometry.Abstractions;

public interface IMassSpectrometryAnalysis
{
    public Task<MassSpecAnalysis> AnalyzeAsync(
        string SampleId,
        string IonizationModem,
        CancellationToken cancellationToken = default
    );
}
