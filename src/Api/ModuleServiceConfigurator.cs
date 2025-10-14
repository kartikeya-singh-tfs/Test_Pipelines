using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using ThermoFisher.Opal.Shared.Registration.Abstractions;
using Wolverine;

namespace ThermoFisher.Opal.Api;

/// <summary>
/// Phase 2 of the module registration process - Service Configuration.
/// This class handles registering services with the dependency injection container and configuring OpenAPI
/// for all registered modules. Once services are configured, calling BuildApplicationConfigurator() commits
/// to the next phase and prevents further service configuration.
///
/// Usage Example:
/// <code>
/// // Configure services for all registered modules
/// serviceConfigurator.ConfigureServices(services, configuration, environment);
///
/// // Configure OpenAPI (typically only in development)
/// if (environment.IsDevelopment())
/// {
///   serviceConfigurator.ConfigureOpenApiServices(services, configuration, environment);
/// }
///
/// // Transition to application configuration phase
/// var appConfigurator = serviceConfigurator.BuildApplicationConfigurator();
/// </code>
/// </summary>
internal partial class ModuleServiceConfigurator
{
    private readonly IReadOnlyList<ModuleRegistrationEntry> _moduleRegistrations;
    private readonly Dictionary<IModule, IReadOnlyCollection<string>> _openApiDocuments = new();
    private readonly ILogger<ModuleServiceConfigurator> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private bool _isCommitted = false;

    /// <summary>
    /// Initializes a new instance of ModuleServiceConfigurator with the registered modules.
    /// This constructor is internal and should only be called by ModuleRegistryBuilder.
    /// </summary>
    /// <param name="moduleRegistrations">List of module registrations from the registration phase.</param>
    /// <param name="logger">Logger instance for capturing registration flow.</param>
    /// <param name="loggerFactory">Logger factory for creating loggers for subsequent phases.</param>
    internal ModuleServiceConfigurator(
        IReadOnlyList<ModuleRegistrationEntry> moduleRegistrations,
        ILoggerFactory loggerFactory
    )
    {
        _moduleRegistrations = moduleRegistrations;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<ModuleServiceConfigurator>();
    }

    /// <summary>
    /// Configures services for all registered modules that implement IServiceRegistrationModule.
    /// This method calls RegisterServices on each module, allowing them to register their dependencies
    /// with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment (Development, Production, etc.).</param>
    /// <exception cref="InvalidOperationException">Thrown when called after BuildApplicationConfigurator().</exception>
    public void ConfigureServices(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        ThrowIfCommitted();

        LogStartingServiceConfiguration();

        foreach (var registration in _moduleRegistrations)
        {
            if (registration.Module is IServiceRegistrationModule serviceModule)
            {
                LogConfiguringServicesForModule(serviceModule.Name);

                var context = new ModuleRegistrationContext(
                    services,
                    configuration,
                    environment,
                    serviceModule.Name
                );
                serviceModule.RegisterServices(context);

                LogSuccessfullyConfiguredServicesForModule(serviceModule.Name);
            }
            else
            {
                LogSkippingServiceConfigurationForModule(registration.Module.Name);
            }
        }

        LogServiceConfigurationCompleted();
    }

    /// <summary>
    /// Configures OpenAPI services for all registered modules that implement IOpenApiRegistrationModule.
    /// This method is typically called only in development environments to set up Swagger documentation.
    /// </summary>
    /// <param name="services">The service collection to register OpenAPI services with.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment (Development, Production, etc.).</param>
    /// <exception cref="InvalidOperationException">Thrown when called after BuildApplicationConfigurator().</exception>
    public void ConfigureOpenApiServices(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        ThrowIfCommitted();

        LogStartingOpenApiConfiguration();

        foreach (var registration in _moduleRegistrations)
        {
            if (registration.Module is IOpenApiRegistrationModule openApiModule)
            {
                LogConfiguringOpenApiForModule(openApiModule.Name);

                var openApiDocuments = openApiModule.RegisterOpenApiServices(
                    new OpenApiServiceRegister(services)
                );
                if (openApiDocuments != null && openApiDocuments.Any())
                {
                    _openApiDocuments[openApiModule] = openApiDocuments;
                    LogRegisteredOpenApiDocumentsForModule(
                        openApiModule.Name,
                        string.Join(", ", openApiDocuments)
                    );
                }
                else
                {
                    LogNoOpenApiDocumentsForModule(openApiModule.Name);
                }
            }
            else
            {
                LogSkippingOpenApiConfigurationForModule(registration.Module.Name);
            }
        }

        LogOpenApiConfigurationCompleted(_openApiDocuments.Values.Sum(docs => docs.Count));
    }

    /// <summary>
    /// Internal implementation of IOpenApiServiceRegister that wraps the service collection
    /// to provide OpenAPI registration capabilities to modules.
    /// </summary>
    private class OpenApiServiceRegister : IOpenApiServiceRegistry
    {
        private readonly IServiceCollection _services;

        /// <summary>
        /// Initializes a new instance of OpenApiServiceRegister.
        /// </summary>
        /// <param name="services">The service collection to register OpenAPI services with.</param>
        public OpenApiServiceRegister(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// Adds OpenAPI service configuration for a specific document.
        /// </summary>
        /// <param name="documentName">The name of the OpenAPI document.</param>
        /// <param name="configureOptions">Action to configure OpenAPI options.</param>
        /// <returns>The same instance for method chaining.</returns>
        public IOpenApiServiceRegistry AddOpenApi(
            string documentName,
            Action<OpenApiOptions> configureOptions
        )
        {
            _services.AddOpenApi(documentName, configureOptions);
            return this;
        }
    }

    /// <summary>
    /// Completes the service configuration phase and transitions to the application configuration phase.
    /// After calling this method, no more service configuration can be performed.
    /// </summary>
    /// <returns>A ModuleApplicationConfigurator for configuring middleware and endpoints in the next phase.</returns>
    /// <remarks>
    /// This method commits the service configuration phase. Any subsequent calls to configuration methods
    /// will throw InvalidOperationException.
    /// </remarks>
    public ModuleApplicationConfigurator BuildApplicationConfigurator()
    {
        _isCommitted = true;
        var applicationConfiguratorLogger =
            _loggerFactory.CreateLogger<ModuleApplicationConfigurator>();
        return new ModuleApplicationConfigurator(
            _moduleRegistrations,
            _openApiDocuments,
            applicationConfiguratorLogger
        );
    }

    /// <summary>
    /// Throws an exception if the configurator has already been committed to the next phase.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the configurator has been committed.</exception>
    private void ThrowIfCommitted()
    {
        if (_isCommitted)
        {
            throw new InvalidOperationException(
                "Cannot configure services after BuildApplicationConfigurator has been called."
            );
        }
    }

    #region Source-Generated Logging Methods

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Starting service configuration for modules"
    )]
    private partial void LogStartingServiceConfiguration();

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Configuring services for module: {ModuleName}"
    )]
    private partial void LogConfiguringServicesForModule(string moduleName);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Successfully configured services for module: {ModuleName}"
    )]
    private partial void LogSuccessfullyConfiguredServicesForModule(string moduleName);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Skipping service configuration for module: {ModuleName} (does not implement IServiceRegistrationModule)"
    )]
    private partial void LogSkippingServiceConfigurationForModule(string moduleName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Service configuration completed")]
    private partial void LogServiceConfigurationCompleted();

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Starting OpenAPI configuration for modules"
    )]
    private partial void LogStartingOpenApiConfiguration();

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Configuring OpenAPI for module: {ModuleName}"
    )]
    private partial void LogConfiguringOpenApiForModule(string moduleName);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Registered OpenAPI documents for module {ModuleName}: [{Documents}]"
    )]
    private partial void LogRegisteredOpenApiDocumentsForModule(
        string moduleName,
        string documents
    );

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "No OpenAPI documents returned for module: {ModuleName}"
    )]
    private partial void LogNoOpenApiDocumentsForModule(string moduleName);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Skipping OpenAPI configuration for module: {ModuleName} (does not implement IOpenApiRegistrationModule)"
    )]
    private partial void LogSkippingOpenApiConfigurationForModule(string moduleName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "OpenAPI configuration completed. Total documents registered: {DocumentCount}"
    )]
    private partial void LogOpenApiConfigurationCompleted(int documentCount);

    #endregion
}
