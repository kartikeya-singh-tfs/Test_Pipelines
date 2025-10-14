using ThermoFisher.Opal.Shared.Registration.Abstractions;

namespace ThermoFisher.Opal.Api;

/// <summary>
/// Represents a registration entry with module and optional path prefix for endpoint routing.
/// </summary>
/// <param name="Module">The module instance implementing IModule interface.</param>
/// <param name="PathPrefix">Optional path prefix for API endpoints. Only used if module implements IHttpEndpointRegistrationModule.</param>
internal record ModuleRegistrationEntry(IModule Module, string? PathPrefix = null);
