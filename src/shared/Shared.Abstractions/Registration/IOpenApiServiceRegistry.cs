using Microsoft.AspNetCore.OpenApi;

namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Interface for registering OpenAPI services.
/// This interface allows modules to register their OpenAPI documents with the application.
/// </summary>
public interface IOpenApiServiceRegistry
{
    /// <summary>
    /// Adds OpenAPI service with the specified document name and configuration.
    /// </summary>
    /// <param name="documentName">The name of the OpenAPI document to register.</param>
    /// <param name="configureOptions">Action to configure OpenAPI options for this document.</param>
    /// <returns>The same IOpenApiServiceRegister instance for method chaining.</returns>
    IOpenApiServiceRegistry AddOpenApi(
        string documentName,
        Action<OpenApiOptions> configureOptions
    );
}
