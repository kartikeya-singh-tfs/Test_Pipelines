using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Context provided to modules during registration phase
/// </summary>
public class ModuleRegistrationContext : ModuleContext
{
    /// <summary>
    /// Gets the service collection used to register services for dependency injection
    /// </summary>
    public IServiceCollection Services { get; init; }

    private IConfiguration _configuration;

    public ModuleRegistrationContext(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string moduleName
    )
        : base(environment, moduleName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        Services = services;
        _configuration = configuration;
    }

    /// <summary>
    /// Gets the module-specific configuration section
    /// </summary>
    public IConfigurationSection GetModuleConfiguration()
    {
        return _configuration.GetSection($"Modules:{ModuleName}");
    }
}
