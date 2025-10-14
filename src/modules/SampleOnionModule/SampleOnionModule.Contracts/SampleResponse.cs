namespace ThermoFisher.SampleOnionModule.Contracts;

/// <summary>
/// Represents a sample in mass chromatography experiment.
/// </summary>
public record SampleResponse(string Id, string Name, string Description, DateTime CollectionDate);
