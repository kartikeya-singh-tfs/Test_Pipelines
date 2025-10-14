using System.Reflection;
using ThermoFisher.Opal.Shared.Registration.Abstractions;
using Wolverine;

namespace ThermoFisher.Opal.Api;

/// <summary>
/// Configures messaging services for all registered modules that implement IMessagingRegistrationModule.
/// </summary>
internal class ModuleMessagingConfigurator
{
    private readonly IReadOnlyList<ModuleRegistrationEntry> _moduleRegistrations;

    public ModuleMessagingConfigurator(IReadOnlyList<ModuleRegistrationEntry> moduleRegistrations)
    {
        _moduleRegistrations =
            moduleRegistrations ?? throw new ArgumentNullException(nameof(moduleRegistrations));
    }

    /// <summary>
    /// Configures messaging services for all registered modules that implement IMessagingRegistrationModule.
    /// This method calls RegisterMessageHandlers on each module, allowing them to register their message handlers with Wolverine.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment (Development, Production, etc.).</param>
    /// <exception cref="InvalidOperationException">Thrown when called after BuildApplicationConfigurator().</exception>
    public IServiceCollection ConfigureMessaging(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddWolverine(opts =>
        {
            // Turn off all logging of the message execution starting and finishing.
            // The default is Debug.
            opts.Policies.MessageExecutionLogLevel(LogLevel.None);

            // Turn down Wolverine's built in logging of all successful.
            // message processing.
            opts.Policies.MessageSuccessLogLevel(LogLevel.None);

            // Configure separate message queue per message type.
            // Each queue will process messages sequentially.
            // With this behavior, if 2 messages of different types are fired one after another, they can be delivered in different order.
            // Customize message queue for each message type to be sequential.
            opts.Policies.ConfigureConventionalLocalRouting()
                .CustomizeQueues(
                    (type, listener) =>
                    {
                        listener.Sequential();
                    }
                );

            var builder = new MessagingRegistrationBuilder(opts);

            foreach (var registration in _moduleRegistrations)
            {
                if (registration.Module is IMessagingRegistrationModule messagingModule)
                {
                    var context = new ModuleRegistrationContext(
                        services,
                        configuration,
                        environment,
                        registration.Module.Name
                    );
                    messagingModule.RegisterMessageHandlers(builder, context);
                }
            }
        });

        return services;
    }

    private class MessagingRegistrationBuilder : IMessagingRegistrationBuilder
    {
        private readonly WolverineOptions _options;

        public MessagingRegistrationBuilder(WolverineOptions options)
        {
            _options = options;
        }

        public void RegisterMessageHandlers(Assembly assembly)
        {
            _options.Discovery.IncludeAssembly(assembly);
        }

        public void RegisterMessageHandlers<T>()
        {
            _options.Discovery.IncludeType<T>();
        }
    }
}
