namespace ThermoFisher.EventRouterModule.Contracts;

/// <summary>
/// Event Router Patterns.
/// </summary>
/// <param name="SessionId">Session Id.</param>
/// <param name="Patterns">Patterns.
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
public record EventRouterPatterns(string SessionId, string[] Patterns);
