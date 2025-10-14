namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Base interface for all modules
/// </summary>
public interface IModule
{
    /// <summary>
    /// Unique identifier for the module
    /// </summary>
    string Name { get; }
}
