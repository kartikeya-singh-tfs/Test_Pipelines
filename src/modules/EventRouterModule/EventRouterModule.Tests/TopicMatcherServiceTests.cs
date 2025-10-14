using NSubstitute.ExceptionExtensions;
using ThermoFisher.EventRouterModule;

namespace EventRouterModule.Tests;

public abstract class TopicMatcherServiceBaseTests
{
    public abstract ITopicMatcherService Service { get; }

    [Theory]
    [InlineData("foo.bar", "foo.bar", true)]
    [InlineData("foo.bar", "foo.baz", false)]
    [InlineData("*", "foo", true)]
    [InlineData("*", "foo.bar", false)]
    [InlineData("*", "", false)]
    [InlineData("#", "foo.bar.baz", true)]
    [InlineData("#", "foo", true)]
    [InlineData("#", "", false)]
    [InlineData("*.bar", "foo.bar", true)]
    [InlineData("*.bar", "bar", false)]
    [InlineData("#.bar", "foo.bar", true)]
    [InlineData("#.bar", "bar", true)]
    [InlineData("#.bar", "some.new.kinda.bar.value", false)]
    [InlineData("*.foo.#", "some.foo", true)]
    [InlineData("*.foo.#", "some.foo.bar", true)]
    [InlineData("*.foo.#", "foo.bar", false)]
    [InlineData("#.foo.#", "some.foo", true)]
    [InlineData("#.foo.#", "some.foo.bar", true)]
    [InlineData("#.foo.#", "foo.bar", true)]
    [InlineData("foo.*", "foo.bar", true)]
    [InlineData("foo.*", "foo.bar.baz", false)]
    [InlineData("foo.*", "foo.", true)]
    [InlineData("foo.*", "foo", false)]
    [InlineData("foo.#", "foo.bar", true)]
    [InlineData("foo.#", "foo.bar.baz", true)]
    [InlineData("foo.#", "foo.", true)]
    [InlineData("foo.#", "foo", true)]
    [InlineData("foo.#", "foo..bar", true)]
    [InlineData("foo.bar.*", "foo.bar.baz", true)]
    [InlineData("foo.bar.*", "foo.bar", false)]
    [InlineData("foo.bar.#", "foo.bar", true)]
    [InlineData("foo.bar.#", "foo.bar.baz", true)]
    [InlineData("foo.bar.#", "foo.bar.baz.qux", true)]
    [InlineData("foo.bar.#", "foo.baz.bar", false)]
    [InlineData("foo.*.bar", "foo.new.other.bar", false)]
    [InlineData("foo.*.bar", "foo.bar", false)]
    [InlineData("foo.*.bar", "foo..bar", true)]
    [InlineData("foo.#.bar", "foo.new.kinda.bar", true)]
    [InlineData("foo.#.bar", "foo.bar", true)]
    [InlineData("foo.#.bar", "some.new.kinda.bar", false)]
    [InlineData("foo.#.bar.*", "foo.new.kinda.bar.hahaha", true)]
    [InlineData("foo.#.bar.#", "foo.new.kinda.bar.hahaha", true)]
    [InlineData("foo.#.bar", "foo..bar", true)]
    public void IsMatch_ReturnsExpectedResult(string pattern, string topic, bool expected)
    {
        var result = Service.IsMatch(pattern, topic);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsMatch_CachesResults()
    {
        var pattern = "foo.*";
        var topic1 = "foo.bar";
        var topic2 = "foo.baz";
        var topic3 = "foo.bar.baz";

        Assert.True(Service.IsMatch(pattern, topic1));
        Assert.True(Service.IsMatch(pattern, topic2));
        Assert.False(Service.IsMatch(pattern, topic3));
    }

    [Fact]
    public void IsMatch_ExactMatchWithoutWildcards()
    {
        Assert.True(Service.IsMatch("abc.def", "abc.def"));
        Assert.False(Service.IsMatch("abc.def", "abc.defg"));
    }

    [Fact]
    public void IsMatch_HandlesEmptyPattern()
    {
        Assert.Throws<ArgumentException>(() => Service.IsMatch("", "foo"));
        Assert.Throws<ArgumentException>(() => Service.IsMatch("", ""));
    }
}

public class TopicMatcherServiceTests : TopicMatcherServiceBaseTests
{
    public override ITopicMatcherService Service => new TopicMatcherService();
}

public class TopicMatcherServiceTestsRegex : TopicMatcherServiceBaseTests
{
    public override ITopicMatcherService Service => new TopicMatcherServiceRegex();
}
