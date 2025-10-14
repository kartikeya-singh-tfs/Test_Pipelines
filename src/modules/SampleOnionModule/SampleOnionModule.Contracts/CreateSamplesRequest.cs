namespace ThermoFisher.SampleOnionModule.Contracts;

/// <summary>
/// Request to create a number of samples.
/// </summary>
public record CreateSamplesRequest(int Count, string NamePrefix, string Description);
