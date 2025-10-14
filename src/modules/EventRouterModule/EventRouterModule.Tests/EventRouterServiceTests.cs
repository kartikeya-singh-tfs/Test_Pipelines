using System.Threading.Channels;
using NSubstitute;
using ThermoFisher.EventRouterModule;
using ThermoFisher.EventRouterModule.Contracts;

namespace EventRouterModule.Tests;

public class EventRouterServiceTests
{
    private EventRouterService CreateService() => new(GetTopicMatcher(), null);

    private ITopicMatcherService GetTopicMatcher()
    {
        var topicMatcher = Substitute.For<ITopicMatcherService>();

        topicMatcher
            .IsMatch(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo =>
            {
                var pattern = callInfo.ArgAt<string>(0);
                var topic = callInfo.ArgAt<string>(1);
                return pattern == topic;
            });

        return topicMatcher;
    }

    private static Channel<EventRouterMessage> CreateChannel() =>
        Channel.CreateUnbounded<EventRouterMessage>();

    [Fact]
    public async Task AddChannel_ShouldAddChannelAndTopics()
    {
        // Arrange
        var service = CreateService();
        var channel = CreateChannel();
        var topics = new HashSet<string> { "topic1", "topic2" };

        // Act
        await service.AddChannel(channel, "ch1", topics, CancellationToken.None);

        // Assert
        // AddTopics should succeed
        await service.AddPatterns("ch1", new HashSet<string> { "topic3" });
    }

    [Fact]
    public async Task RemoveChannel_ShouldRemoveChannelAndTopics()
    {
        // Arrange
        var service = CreateService();
        var channel = CreateChannel();
        var topics = new HashSet<string> { "topic1" };

        // Act
        await service.AddChannel(channel, "ch1", topics, CancellationToken.None);
        await service.RemoveChannel(channel, "ch1");

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.AddPatterns("ch1", new HashSet<string> { "topic2" })
        );
    }

    [Fact]
    public async Task AddTopics_ShouldAddTopicsToExistingChannel()
    {
        // Arrange
        var service = CreateService();
        var channel = CreateChannel();
        var topics = new HashSet<string> { "topic1" };

        // Act
        await service.AddChannel(channel, "ch1", topics, CancellationToken.None);
        await service.AddPatterns("ch1", new HashSet<string> { "topic2", "topic3" });

        // Assert
        // RemoveTopics should remove topic2
        await service.RemovePatterns("ch1", new HashSet<string> { "topic2" });
    }

    [Fact]
    public async Task AddTopics_ShouldThrowIfChannelNotFound()
    {
        // Arrange
        var service = CreateService();

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.AddPatterns("missing", new HashSet<string> { "topic" })
        );
    }

    [Fact]
    public async Task RemoveTopics_ShouldThrowIfChannelNotFound()
    {
        // Arrange
        var service = CreateService();

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.RemovePatterns("missing", new HashSet<string> { "topic" })
        );
    }

    [Fact]
    public async Task OnMessage_ShouldWriteToCorrectChannels()
    {
        // Arrange
        var service = CreateService();
        var channel1 = CreateChannel();
        var channel2 = CreateChannel();
        var topics1 = new HashSet<string> { "topicA" };
        var topics2 = new HashSet<string> { "topicB" };

        // Act
        await service.AddChannel(channel1, "ch1", topics1, CancellationToken.None);
        await service.AddChannel(channel2, "ch2", topics2, CancellationToken.None);

        var messageA = new EventRouterMessage("topicA", "dataA");
        var messageB = new EventRouterMessage("topicB", "dataB");

        await service.OnMessage(messageA);
        await service.OnMessage(messageB);

        var readA = await channel1.Reader.ReadAsync();

        // Assert
        Assert.Equal("topicA", readA.Topic);
        Assert.Equal("dataA", readA.Data);

        var readB = await channel2.Reader.ReadAsync();

        // Assert
        Assert.Equal("topicB", readB.Topic);
        Assert.Equal("dataB", readB.Data);
    }

    [Fact]
    public async Task OnMessage_ShouldNotWriteIfTopicNotSubscribed()
    {
        // Arrange
        var service = CreateService();
        var channel = CreateChannel();
        var topics = new HashSet<string> { "topicA" };

        // Act
        await service.AddChannel(channel, "ch1", topics, CancellationToken.None);

        var message = new EventRouterMessage("topicB", "data");
        await service.OnMessage(message);

        // Assert
        Assert.False(channel.Reader.TryRead(out _));
    }
}
