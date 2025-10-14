namespace ThermoFisher.SampleOnionModule.Abstractions;

public record Sample(
    string Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<string> Tags
);
