namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Interface for modules that register OpenAPI services.
/// Implement this interface to configure and register OpenAPI services for a specific module.
/// </summary>
public interface IOpenApiRegistrationModule : IModule
{
    /// <summary>
    /// Registers OpenAPI services for this module.
    /// </summary>
    /// <param name="openApiServiceRegister">The service responsible for registering OpenAPI services.</param>
    /// <returns>A read-only collection containing the names of the OpenAPI documents registered by the module.</returns>
    IReadOnlyCollection<string> RegisterOpenApiServices(
        IOpenApiServiceRegistry openApiServiceRegister
    );
}
