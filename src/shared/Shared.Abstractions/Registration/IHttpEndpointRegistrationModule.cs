namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Interface for modules that provide REST API endpoints
/// </summary>
public interface IHttpEndpointRegistrationModule : IModule
{
    /// <summary>
    /// Registers HTTP endpoints for this module using the provided endpoint builder.
    /// </summary>
    /// <param name="endpoints">The endpoint builder used to register HTTP endpoints.</param>
    /// <param name="context">The module startup context containing service provider and environment information.</param>
    void RegisterEndpoints(IHttpEndpointBuilder endpoints, ModuleStartupContext context);
}
