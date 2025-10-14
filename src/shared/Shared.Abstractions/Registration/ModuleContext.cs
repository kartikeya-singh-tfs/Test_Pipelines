using Microsoft.Extensions.Hosting;

namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Abstract base context for modules
/// </summary>
public abstract class ModuleContext
{
    /// <summary>
    /// Gets the hosting environment information (Development, Production, etc.)
    /// </summary>
    public IHostEnvironment Environment { get; init; }

    /// <summary>
    /// Gets the name of the module this context belongs to
    /// </summary>
    public string ModuleName { get; init; }

    protected ModuleContext(IHostEnvironment environment, string moduleName)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(moduleName);
        Environment = environment;
        ModuleName = moduleName;
    }
}
