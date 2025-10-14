namespace ThermoFisher.SampleVerticalModule.MassSpectrometry.Contracts;

public record MassSpecAnalysisResponse(
    string SampleId,
    string IonizationMode,
    double[] MassRange,
    int SpectraCount,
    double BasePeakIntensity,
    DateTime AnalyzedAt
);
