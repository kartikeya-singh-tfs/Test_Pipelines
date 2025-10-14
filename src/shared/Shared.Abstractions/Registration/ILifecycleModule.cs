namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Interface for modules that need to respond to application lifecycle events
/// </summary>
public interface ILifecycleModule : IModule
{
    /// <summary>
    /// Called when the application has started
    /// </summary>
    Task OnApplicationStartedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Called when the application has stopped
    /// </summary>
    Task OnApplicationStoppedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    );
}
