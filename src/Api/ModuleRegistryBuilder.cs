using System.Collections.ObjectModel;
using ThermoFisher.Opal.Shared.Registration.Abstractions;

namespace ThermoFisher.Opal.Api;

/// <summary>
/// Phase 1 of the module registration process - Module Registration Builder.
/// This class provides a fluent API for registering modules with optional path prefixes.
/// Modules implementing IHttpEndpointRegistrationModule must be registered with path prefixes,
/// while other modules can be registered without them.
/// Once modules are registered, calling BuildServiceConfigurator() commits to the next phase
/// and prevents further module registrations.
/// </summary>
internal partial class ModuleRegistryBuilder
{
    private readonly List<ModuleRegistrationEntry> _moduleRegistrations = new();
    private bool _isCommitted = false;
    private readonly ILogger<ModuleRegistryBuilder> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public ModuleRegistryBuilder(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = _loggerFactory.CreateLogger<ModuleRegistryBuilder>();
    }

    /// <summary>
    /// Registers a module instance without a path prefix. Use this for modules that don't implement IHttpEndpointRegistrationModule.
    /// </summary>
    /// <param name="module">The module instance to register. Must implement IModule and optionally other module interfaces.</param>
    /// <returns>The same ModuleRegistryBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when module is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a module with the same name is already registered, when called after BuildServiceConfigurator(), or when module implements IHttpEndpointRegistrationModule (path prefix required).</exception>
    public ModuleRegistryBuilder RegisterModule(IModule module)
    {
        ThrowIfCommitted();

        if (module == null)
        {
            throw new ArgumentNullException(nameof(module), "Module cannot be null");
        }

        if (module is IHttpEndpointRegistrationModule)
        {
            throw new InvalidOperationException(
                $"Module '{module.Name}' implements IHttpEndpointRegistrationModule and must be registered with a path prefix. Use RegisterModule(module, pathPrefix) instead."
            );
        }

        if (_moduleRegistrations.Any(r => r.Module.Name == module.Name))
        {
            throw new InvalidOperationException(
                $"A module with the name '{module.Name}' is already registered."
            );
        }

        _moduleRegistrations.Add(new ModuleRegistrationEntry(module));
        return this;
    }

    /// <summary>
    /// Registers a module instance with a custom API path prefix for endpoint routing.
    /// Use this for modules that implement IHttpEndpointRegistrationModule.
    /// </summary>
    /// <param name="module">The module instance to register. Must implement IModule and optionally other module interfaces.</param>
    /// <param name="pathPrefix">The custom path prefix for the module's API endpoints (e.g., "users" results in "/api/users/").</param>
    /// <returns>The same ModuleRegistryBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when module is null.</exception>
    /// <exception cref="ArgumentException">Thrown when pathPrefix is null, empty, or not a valid relative URI.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a module with the same name or path prefix is already registered, or when called after BuildServiceConfigurator().</exception>
    public ModuleRegistryBuilder RegisterModule(IModule module, string pathPrefix)
    {
        ThrowIfCommitted();

        if (module == null)
        {
            throw new ArgumentNullException(nameof(module), "Module cannot be null");
        }

        if (string.IsNullOrWhiteSpace(pathPrefix))
        {
            throw new ArgumentException("Path prefix cannot be null or empty", nameof(pathPrefix));
        }

        if (!Uri.IsWellFormedUriString(pathPrefix, UriKind.Relative))
        {
            throw new ArgumentException(
                "Path prefix must be a valid relative URI",
                nameof(pathPrefix)
            );
        }

        if (_moduleRegistrations.Any(r => r.Module.Name == module.Name))
        {
            throw new InvalidOperationException(
                $"A module with the name '{module.Name}' is already registered."
            );
        }

        if (
            _moduleRegistrations.Any(r =>
                r.PathPrefix != null
                && r.PathPrefix.Equals(pathPrefix, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            throw new InvalidOperationException(
                $"A module with the path prefix '{pathPrefix}' is already registered."
            );
        }

        _moduleRegistrations.Add(new ModuleRegistrationEntry(module, pathPrefix));
        LogModuleRegisteredWithPathPrefix(module.Name, pathPrefix);
        return this;
    }

    /// <summary>
    /// Registers a module by type without a path prefix. Use this for modules that don't implement IHttpEndpointRegistrationModule.
    /// The module type must have a parameterless constructor.
    /// </summary>
    /// <typeparam name="T">The module type to instantiate and register. Must implement IModule and have a parameterless constructor.</typeparam>
    /// <returns>The same ModuleRegistryBuilder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a module with the same name is already registered, when called after BuildServiceConfigurator(), or when module implements IHttpEndpointRegistrationModule (path prefix required).</exception>
    public ModuleRegistryBuilder RegisterModule<T>()
        where T : class, IModule, new()
    {
        var module = new T();
        return RegisterModule(module);
    }

    /// <summary>
    /// Registers a module by type with a custom API path prefix for endpoint routing.
    /// Use this for modules that implement IHttpEndpointRegistrationModule.
    /// The module type must have a parameterless constructor.
    /// </summary>
    /// <typeparam name="T">The module type to instantiate and register. Must implement IModule and have a parameterless constructor.</typeparam>
    /// <param name="pathPrefix">The custom path prefix for the module's API endpoints (e.g., "users" results in "/api/users/").</param>
    /// <returns>The same ModuleRegistryBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when pathPrefix is null, empty, or not a valid relative URI.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a module with the same name or path prefix is already registered, or when called after BuildServiceConfigurator().</exception>
    public ModuleRegistryBuilder RegisterModule<T>(string pathPrefix)
        where T : class, IModule, new()
    {
        var module = new T();
        return RegisterModule(module, pathPrefix);
    }

    public ModuleDbConfigurator BuildDbConfigurator()
    {
        return new ModuleDbConfigurator(ModuleRegistrations, new SecretStore());
    }

    /// <summary>
    /// All registered modules
    /// </summary>
    internal ReadOnlyCollection<ModuleRegistrationEntry> ModuleRegistrations =>
        _moduleRegistrations.AsReadOnly();

    /// <summary>
    /// Completes the module registration phase and transitions to the service configuration phase.
    /// After calling this method, no more modules can be registered.
    /// </summary>
    /// <returns>A ModuleServiceConfigurator for configuring services and OpenAPI in the next phase.</returns>
    /// <remarks>
    /// This method commits the module registration phase. Any subsequent calls to RegisterModule methods
    /// will throw InvalidOperationException.
    /// </remarks>
    public ModuleServiceConfigurator BuildServiceConfigurator()
    {
        _isCommitted = true;
        return new ModuleServiceConfigurator(_moduleRegistrations.AsReadOnly(), _loggerFactory);
    }

    /// <summary>
    /// Builds a ModuleMessagingConfigurator for configuring messaging in the service configuration phase.
    /// </summary>
    /// <returns></returns>
    public ModuleMessagingConfigurator BuildMessagingConfigurator()
    {
        return new ModuleMessagingConfigurator(_moduleRegistrations.AsReadOnly());
    }

    /// <summary>
    /// Throws an exception if the builder has already been committed to the next phase.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the builder has been committed.</exception>
    private void ThrowIfCommitted()
    {
        if (_isCommitted)
        {
            throw new InvalidOperationException(
                "Cannot register modules after BuildServiceConfigurator has been called."
            );
        }
    }

    #region Source-Generated Logging Methods

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Module registered: {ModuleName} with path prefix: {PathPrefix}"
    )]
    private partial void LogModuleRegisteredWithPathPrefix(string moduleName, string pathPrefix);

    #endregion
}
