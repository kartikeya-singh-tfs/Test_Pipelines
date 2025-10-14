using System.Reflection;

namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Interface for building messaging registrations in modules.
///
/// <remarks>
/// By default, Wolverine is looking for public, concrete classes that follow any of these rules:
/// <list type="bullet">
/// <item><description>Implements the Wolverine.IWolverineHandler interface</description></item>
/// <item><description>Is decorated with the[Wolverine.WolverineHandler] attribute</description></item>
/// <item><description>Type name ends with "Handler"</description></item>
/// <item><description>Type name ends with "Consumer"</description></item>
/// </list>
///
/// From the types, by default, Wolverine looks for any public instance method that is:
/// <list type="bullet">
/// <item><description>Is named Handle, Handles, Consume, Consumes or one of the names from Wolverine's saga support</description></item>
/// <item><description>Is decorated by the[WolverineHandler] attribute if you want to use a different, descriptive name</description></item>
/// </list>
///
/// In all cases, Wolverine assumes that the first argument is the incoming message.
///
/// <para>
/// See <see href="https://wolverinefx.net/guide/handlers/discovery.html#snippet-sample_ValidMessageHandlers">
/// the Wolverine documentation on message handler discovery</see> for details
/// </para>
/// </remarks>
/// </summary>
public interface IMessagingRegistrationBuilder
{
    /// <summary>
    /// Includes all message handlers from the specified assembly.
    /// </summary>
    /// <param name="assembly"></param>
    void RegisterMessageHandlers(Assembly assembly);

    /// <summary>
    /// Includes all message handlers from the type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    void RegisterMessageHandlers<T>();
}
