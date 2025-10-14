using Microsoft.AspNetCore.Builder;

namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Interface for building gRPC endpoints in modules.
/// </summary>
public interface IGrpcEndpointBuilder
{
    /// <summary>
    /// Maps a gRPC service to the endpoint routing system.
    /// </summary>
    /// <typeparam name="TService">The type of the gRPC service to map.</typeparam>
    /// <returns>An IEndpointConventionBuilder for further configuration of the gRPC service endpoint.</returns>
    IEndpointConventionBuilder MapGrpcService<TService>()
        where TService : class;
}
