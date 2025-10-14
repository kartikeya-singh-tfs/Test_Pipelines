namespace ThermoFisher.SampleOnionModule.Abstractions;

public record Compound(
    string Name,
    string Formula,
    string Description,
    double PeakArea,
    double RetentionTime,
    double Concentration,
    IEnumerable<string> Tags,
    int score
);
