namespace ThermoFisher.EventRouterModule.Contracts;

/// <summary>
/// Represents a event router message with a topic and associated data.
/// </summary>
/// <param name="Topic">
/// <remarks>
/// <para>
/// Topic must be a list of words, delimited by dots.
/// Each word can be any empty or non-empty string without *, # and .
/// </para>
/// </remarks>
/// </param>
/// <param name="Data">Data.</param>
public record EventRouterMessage(string Topic, object Data);
