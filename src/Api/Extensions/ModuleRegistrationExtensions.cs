using ThermoFisher.AcquisitionModule.Opal;
using ThermoFisher.EventRouterModule;
using ThermoFisher.SampleOnionModule.Opal;
using ThermoFisher.SampleVerticalModule.Opal;

namespace ThermoFisher.Opal.Api.Extensions;

/// <summary>
/// Extensions for registering application modules.
/// </summary>
internal static class ModuleRegistry
{
    /// <summary>
    /// Registers all application modules with the module registry builder.
    /// </summary>
    /// <param name="moduleBuilder">The module registry builder to register modules with.</param>
    /// <returns>The module registry builder for method chaining.</returns>
    public static ModuleRegistryBuilder RegisterAllModules(this ModuleRegistryBuilder moduleBuilder)
    {
        return moduleBuilder
            .RegisterModule<EventRouterModuleRegistration>("sse")
            .RegisterModule<AcquisitionModuleRegistration>("acquisition")
            .RegisterModule<SampleOnionModuleRegistration>("onion") //TODO: Remove this and appsettings.json entries, once sample modules are removed
            .RegisterModule<SampleVerticalModuleRegistration>("vertical"); //TODO: Remove this and appsettings.json entries, once sample modules are removed
    }
}
