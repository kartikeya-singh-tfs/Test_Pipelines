namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Interface for modules that need to register services
/// </summary>
public interface IServiceRegistrationModule : IModule
{
    /// <summary>
    /// Register services with the DI container
    /// </summary>
    void RegisterServices(ModuleRegistrationContext context);
}
