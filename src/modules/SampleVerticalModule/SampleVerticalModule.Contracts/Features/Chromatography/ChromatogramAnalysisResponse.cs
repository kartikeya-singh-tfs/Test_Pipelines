namespace ThermoFisher.SampleVerticalModule.Chromatography.Contracts;

public record ChromatogramAnalysisResponse(
    string SampleId,
    string MethodName,
    int PeakCount,
    double RetentionTimeRange,
    DateTime AnalyzedAt
);
