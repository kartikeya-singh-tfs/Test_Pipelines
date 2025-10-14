namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Interface for modules that register message handlers.
/// </summary>
public interface IMessagingRegistrationModule
{
    /// <summary>
    /// Register message handlers using provided messaging registration builder.
    /// </summary>
    /// <param name="builder">The messaging registration builder.</param>
    /// <param name="context">The module registration context.</param>
    void RegisterMessageHandlers(
        IMessagingRegistrationBuilder builder,
        ModuleRegistrationContext context
    );
}
