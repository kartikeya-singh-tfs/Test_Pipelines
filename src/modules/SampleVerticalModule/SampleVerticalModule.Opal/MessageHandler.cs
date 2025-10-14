using Microsoft.Extensions.Logging;
using SampleOnionModule.Contracts;

namespace ThermoFisher.SampleVerticalModule.Opal;

/// <summary>
/// Message handlers for processing incoming messages.
/// </summary>
// By default, Wolverine is looking for public, concrete classes that follow any of these rules:
// Implements the Wolverine.IWolverineHandler interface
// Is decorated with the[Wolverine.WolverineHandler] attribute
// Type name ends with "Handler"
// Type name ends with "Consumer"
//
// From the types, by default, Wolverine looks for any public instance method that is:
// Is named Handle, Handles, Consume, Consumes or one of the names from Wolverine's saga support
// Is decorated by the[WolverineHandler] attribute if you want to use a different, descriptive name
//
// In all cases, Wolverine assumes that the first argument is the incoming message.
//
// https://wolverinefx.net/guide/handlers/discovery.html#snippet-sample_ValidMessageHandlers
public class MessageHandler
{
    /// <summary>
    /// Handles AddRequest messages by returning an AddResponse with the sum of X and Y.
    /// </summary>
    /// <param name="add"></param>
    /// <returns></returns>
    public Task<AddResponse> HandleAsync(AddRequest add, ILogger<MessageHandler>? logger)
    {
        logger?.LogInformation(
            "In MessageHandler.HandleAsync for AddRequest. X:{X} Y:{Y}",
            add.X,
            add.Y
        );
        return Task.FromResult(new AddResponse(add.X + add.Y));
    }

    /// <summary>
    /// Handles SendRequestMessage messages. This method is a placeholder and does not perform any action.
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public Task HandleAsync(SendRequestMessage msg, ILogger<MessageHandler>? logger)
    {
        logger?.LogInformation(
            "In MessageHandler.HandleAsync for SendRequestMessage. A:{A}",
            msg.A
        );
        return Task.CompletedTask;
    }
}
