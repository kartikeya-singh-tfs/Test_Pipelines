using System.Threading.Channels;
using ThermoFisher.EventRouterModule.Contracts;

namespace ThermoFisher.EventRouterModule;

/// <summary>
/// Service for managing event router channels and patterns.
///
/// <remarks>
/// <para>
/// Pattern must be a list of words, delimited by dots.
/// Each word can be one of the following
/// <list type="bullet">
/// <item><description>Any empty or non-empty string without *, # and .</description></item>
/// <item><description>Wildcard *. * (star) can substitute for exactly one word.</description></item>
/// <item><description>Wildcard #. # (hash) can substitute for zero or more words.</description></item>
/// </list>
/// </para>
/// </remarks>
/// </summary>
public interface IEventRouterService
{
    /// <summary>
    /// Adds a new event router channel with the specified ID and patterns.
    /// </summary>
    /// <param name="channel">Channel.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="patterns">Patterns to add.</param>
    /// <returns>Task</returns>
    Task AddChannel(
        Channel<EventRouterMessage> channel,
        string sessionId,
        HashSet<string> patterns,
        CancellationToken token
    );

    /// <summary>
    /// Removes an existing event router channel by its ID.
    /// </summary>
    /// <param name="channel">Channel.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <returns>Task</returns>
    Task RemoveChannel(Channel<EventRouterMessage> channel, string sessionId);

    /// <summary>
    /// Adds patterns to an existing event router channel.
    /// </summary>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="patterns">Patterns to add.</param>
    /// <returns>Task</returns>
    Task AddPatterns(string sessionId, HashSet<string> patterns);

    /// <summary>
    /// Removes patterns from an existing event router channel.
    /// </summary>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="patterns">Patterns to remove.</param>
    /// <returns>Task</returns>
    Task RemovePatterns(string sessionId, HashSet<string> patterns);

    /// <summary>
    /// Handles an incoming event router message and distributes it to the appropriate channels based on their subscribed patterns.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <returns>Task</returns>
    Task OnMessage(EventRouterMessage message);
}
