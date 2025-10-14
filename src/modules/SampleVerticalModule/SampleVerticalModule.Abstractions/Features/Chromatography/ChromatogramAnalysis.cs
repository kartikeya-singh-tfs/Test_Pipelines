namespace ThermoFisher.SampleVerticalModule.Chromatography.Abstractions;

public record ChromatogramAnalysis(
    string SampleId,
    string MethodName,
    int PeakCount,
    double RetentionTimeRange,
    string Status,
    DateTime AnalyzedAt
);
