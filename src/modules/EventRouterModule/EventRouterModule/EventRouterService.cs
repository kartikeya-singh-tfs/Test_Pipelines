using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using ThermoFisher.EventRouterModule.Contracts;

namespace ThermoFisher.EventRouterModule;

///<inheritdoc/>
internal class EventRouterService(ITopicMatcherService matcher, ILogger<EventRouterService> logger)
    : IEventRouterService
{
    // sessionId = ChannelPatterns
    private readonly ConcurrentDictionary<string, ChannelPatterns> _channels = new();

    ///<inheritdoc/>
    public Task AddChannel(
        Channel<EventRouterMessage> channel,
        string sessionId,
        HashSet<string> patterns,
        CancellationToken token
    )
    {
        if (_channels.ContainsKey(sessionId))
        {
            throw new ArgumentException($"Session Id {sessionId} already exists.");
        }

        _channels[sessionId] = new(channel, [.. patterns], token);
        return Task.CompletedTask;
    }

    ///<inheritdoc/>
    public Task RemoveChannel(Channel<EventRouterMessage> channel, string sessionId)
    {
        _channels.TryRemove(sessionId, out _);
        return Task.CompletedTask;
    }

    ///<inheritdoc/>
    public Task AddPatterns(string sessionId, HashSet<string> patterns)
    {
        if (_channels.TryGetValue(sessionId, out var channelPatterns))
        {
            channelPatterns.Patterns.UnionWith(patterns);
            return Task.CompletedTask;
        }
        else
        {
            throw new KeyNotFoundException($"Session Id {sessionId} not found.");
        }
    }

    ///<inheritdoc/>
    public Task RemovePatterns(string sessionId, HashSet<string> patterns)
    {
        if (_channels.TryGetValue(sessionId, out var channelPatterns))
        {
            channelPatterns.Patterns.ExceptWith(patterns);
            return Task.CompletedTask;
        }
        else
        {
            throw new KeyNotFoundException($"Session Id {sessionId} not found.");
        }
    }

    ///<inheritdoc/>
    public async Task OnMessage(EventRouterMessage message)
    {
        await Parallel.ForEachAsync(
            _channels,
            async (kvp, token) =>
            {
                foreach (var pattern in kvp.Value.Patterns)
                {
                    if (matcher.IsMatch(pattern, message.Topic))
                    {
                        try
                        {
                            var ct = CancellationTokenSource
                                .CreateLinkedTokenSource(token, kvp.Value.Token)
                                .Token;
                            await kvp.Value.Channel.Writer.WriteAsync(message, ct);
                        }
                        catch (OperationCanceledException)
                        {
                            logger?.LogInformation(
                                "Write message to channel for session {SessionId} cancelled",
                                kvp.Key
                            );
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError(
                                ex,
                                "Failed to write message to channel for session {SessionId}",
                                kvp.Key
                            );
                        }
                        break;
                    }
                }
            }
        );
    }

    private record ChannelPatterns(
        Channel<EventRouterMessage> Channel,
        ConcurrentHashSet<string> Patterns,
        CancellationToken Token
    );
}
