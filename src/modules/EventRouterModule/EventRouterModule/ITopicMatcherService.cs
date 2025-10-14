namespace ThermoFisher.EventRouterModule;

/// <summary>
/// Service to match topics against patterns with wildcards.
/// </summary>
public interface ITopicMatcherService
{
    /// <summary>
    /// Check if a topic matches a pattern.
    /// </summary>
    /// <param name="pattern">The pattern.
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
    /// </param>
    /// <param name="topic">The topic.
    /// <remarks>
    /// <para>
    /// Topic must be a list of words, delimited by dots.
    /// Each word can be any empty or non-empty string without *, # and .
    /// </para>
    /// </remarks>
    /// </param>
    /// <returns>True if topic matches the pattern. Else false.</returns>
    bool IsMatch(string pattern, string topic);
}
