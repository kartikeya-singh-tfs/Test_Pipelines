using ThermoFisher.EventRouterModule.Contracts;

namespace ThermoFisher.EventRouterModule;

/// <summary>
/// Handler for processing event router messages.
/// </summary>
public class EventRouterMessageHandler
{
    /// <summary>
    /// Handles an incoming event router message by delegating to the IEventRouterService to process and distribute the message.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="router">Rvent router Service.</param>
    /// <returns>Task</returns>
    public async Task HandleAsync(EventRouterMessage message, IEventRouterService router)
    {
        await router.OnMessage(message);
    }
}
