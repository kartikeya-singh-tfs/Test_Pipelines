using Microsoft.Extensions.Hosting;

namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Context provided to modules during application startup
/// </summary>
public class ModuleStartupContext : ModuleContext
{
    /// <summary>
    /// Gets the service provider used to resolve services from the dependency injection container
    /// </summary>
    public IServiceProvider ServiceProvider { get; init; }

    public ModuleStartupContext(
        IServiceProvider serviceProvider,
        IHostEnvironment environment,
        string moduleName
    )
        : base(environment, moduleName)
    {
        ServiceProvider = serviceProvider;
    }
}
