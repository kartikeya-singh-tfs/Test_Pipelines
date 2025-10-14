namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Interface for modules that provide gRPC endpoints.
/// </summary>
public interface IGrpcRegistrationModule : IModule
{
    /// <summary>
    /// Registers gRPC endpoints for this module using the provided endpoint builder.
    /// </summary>
    /// <param name="endpoints">The gRPC endpoint builder used to register gRPC services.</param>
    /// <param name="context">The module startup context containing service provider and environment information.</param>
    void RegisterGrpcEndpoints(IGrpcEndpointBuilder endpoints, ModuleStartupContext context);
}
