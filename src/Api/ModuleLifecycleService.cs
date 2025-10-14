namespace ThermoFisher.Opal.Api;

/// <summary>
/// Hosted service that manages the lifecycle of registered modules.
/// This service automatically calls lifecycle methods on modules that implement ILifecycleModule
/// when the application starts up and shuts down, ensuring proper initialization and cleanup.
///
/// The service integrates with ASP.NET Core's hosted service infrastructure and will be
/// automatically started and stopped by the application host.
/// </summary>
internal class ModuleLifecycleService : IHostedService
{
    private readonly ModuleApplicationConfigurator _appConfigurator;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of ModuleLifecycleService.
    /// </summary>
    /// <param name="appConfigurator">The module application configurator that provides access to registered modules.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies during lifecycle events.</param>
    /// <exception cref="ArgumentNullException">Thrown when appConfigurator or serviceProvider is null.</exception>
    public ModuleLifecycleService(
        ModuleApplicationConfigurator appConfigurator,
        IServiceProvider serviceProvider
    )
    {
        _appConfigurator =
            appConfigurator ?? throw new ArgumentNullException(nameof(appConfigurator));
        _serviceProvider =
            serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Called when the application host is ready to start the service.
    /// This method invokes OnApplicationStartedAsync on all registered modules that implement ILifecycleModule.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the startup operation.</param>
    /// <returns>A task representing the asynchronous startup operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _appConfigurator.HandleApplicationEventsAsync(
            _serviceProvider,
            (module, sp, ct) => module.OnApplicationStartedAsync(sp, ct),
            cancellationToken
        );
    }

    /// <summary>
    /// Called when the application host is performing a graceful shutdown.
    /// This method invokes OnApplicationStoppedAsync on all registered modules that implement ILifecycleModule.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the shutdown operation.</param>
    /// <returns>A task representing the asynchronous shutdown operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _appConfigurator.HandleApplicationEventsAsync(
            _serviceProvider,
            (module, sp, ct) => module.OnApplicationStoppedAsync(sp, ct),
            cancellationToken
        );
    }
}
