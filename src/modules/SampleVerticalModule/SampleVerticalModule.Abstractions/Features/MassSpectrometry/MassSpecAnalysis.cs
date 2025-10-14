namespace ThermoFisher.SampleVerticalModule.MassSpectrometry.Abstractions;

public record MassSpecAnalysis(
    string SampleId,
    string IonizationMode,
    double[] MassRange,
    int SpectraCount,
    double BasePeakIntensity,
    string Status,
    DateTime AnalyzedAt
);
