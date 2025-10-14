using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Patterns;

namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Provides methods for mapping HTTP endpoints in a module's API.
/// This interface extends IEndpointConventionBuilder to provide a fluent API for defining REST endpoints
/// with support for all HTTP methods, route patterns, and endpoint grouping.
/// </summary>
public interface IHttpEndpointBuilder : IEndpointConventionBuilder
{
    /// <summary>
    /// Maps an endpoint with the specified route pattern and request delegate.
    /// </summary>
    /// <param name="pattern">The route pattern for the endpoint.</param>
    /// <param name="requestDelegate">The request delegate to execute for this endpoint.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder Map(RoutePattern pattern, RequestDelegate requestDelegate);

    /// <summary>
    /// Maps an endpoint with the specified route pattern and delegate.
    /// </summary>
    /// <param name="pattern">The route pattern for the endpoint.</param>
    /// <param name="requestDelegate">The delegate to execute for this endpoint.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder Map(RoutePattern pattern, Delegate requestDelegate);

    /// <summary>
    /// Maps an endpoint with the specified route pattern and request delegate.
    /// </summary>
    /// <param name="pattern">The route pattern string for the endpoint.</param>
    /// <param name="requestDelegate">The request delegate to execute for this endpoint.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder Map(string pattern, RequestDelegate requestDelegate);

    /// <summary>
    /// Maps an endpoint with the specified route pattern and delegate.
    /// </summary>
    /// <param name="pattern">The route pattern string for the endpoint.</param>
    /// <param name="requestDelegate">The delegate to execute for this endpoint.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder Map(string pattern, Delegate requestDelegate);

    /// <summary>
    /// Maps an endpoint that accepts the specified HTTP methods.
    /// </summary>
    /// <param name="pattern">The route pattern for the endpoint.</param>
    /// <param name="httpMethods">The HTTP methods this endpoint accepts (e.g., GET, POST, PUT).</param>
    /// <param name="requestDelegate">The request delegate to execute for this endpoint.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapMethods(
        string pattern,
        IEnumerable<string> httpMethods,
        RequestDelegate requestDelegate
    );

    /// <summary>
    /// Maps an endpoint that accepts the specified HTTP methods.
    /// </summary>
    /// <param name="pattern">The route pattern for the endpoint.</param>
    /// <param name="httpMethods">The HTTP methods this endpoint accepts (e.g., GET, POST, PUT).</param>
    /// <param name="requestDelegate">The delegate to execute for this endpoint.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapMethods(
        string pattern,
        IEnumerable<string> httpMethods,
        Delegate requestDelegate
    );

    /// <summary>
    /// Maps a GET endpoint with the specified route pattern.
    /// </summary>
    /// <param name="pattern">The route pattern for the endpoint.</param>
    /// <param name="handler">The delegate to execute for GET requests.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapGet(string pattern, Delegate handler);

    /// <summary>
    /// Maps a GET endpoint with the specified route pattern.
    /// </summary>
    /// <param name="pattern">The route pattern for the endpoint.</param>
    /// <param name="handler">The request delegate to execute for GET requests.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapGet(string pattern, RequestDelegate handler);

    /// <summary>
    /// Maps a POST endpoint with the specified route pattern.
    /// </summary>
    /// <param name="pattern">The route pattern for the endpoint.</param>
    /// <param name="handler">The delegate to execute for POST requests.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapPost(string pattern, Delegate handler);

    /// <summary>
    /// Maps a POST endpoint with the specified route pattern.
    /// </summary>
    /// <param name="pattern">The route pattern for the endpoint.</param>
    /// <param name="handler">The request delegate to execute for POST requests.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapPost(string pattern, RequestDelegate handler);

    /// <summary>
    /// Maps a PUT endpoint with the specified route pattern.
    /// </summary>
    /// <param name="pattern">The route pattern for the endpoint.</param>
    /// <param name="handler">The delegate to execute for PUT requests.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapPut(string pattern, Delegate handler);

    /// <summary>
    /// Maps a PUT endpoint with the specified route pattern.
    /// </summary>
    /// <param name="pattern">The route pattern for the endpoint.</param>
    /// <param name="handler">The request delegate to execute for PUT requests.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapPut(string pattern, RequestDelegate handler);

    /// <summary>
    /// Maps a DELETE endpoint with the specified route pattern.
    /// </summary>
    /// <param name="pattern">The route pattern for the endpoint.</param>
    /// <param name="handler">The delegate to execute for DELETE requests.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapDelete(string pattern, Delegate handler);

    /// <summary>
    /// Maps a DELETE endpoint with the specified route pattern.
    /// </summary>
    /// <param name="pattern">The route pattern for the endpoint.</param>
    /// <param name="handler">The request delegate to execute for DELETE requests.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapDelete(string pattern, RequestDelegate handler);

    /// <summary>
    /// Maps a PATCH endpoint with the specified route pattern.
    /// </summary>
    /// <param name="pattern">The route pattern for the endpoint.</param>
    /// <param name="handler">The delegate to execute for PATCH requests.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapPatch(string pattern, Delegate handler);

    /// <summary>
    /// Maps a PATCH endpoint with the specified route pattern.
    /// </summary>
    /// <param name="pattern">The route pattern for the endpoint.</param>
    /// <param name="handler">The request delegate to execute for PATCH requests.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapPatch(string pattern, RequestDelegate handler);

    /// <summary>
    /// Maps a fallback endpoint that handles requests that don't match any other endpoints.
    /// </summary>
    /// <param name="handler">The delegate to execute for fallback requests.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapFallback(Delegate handler);

    /// <summary>
    /// Maps a fallback endpoint with a specific pattern that handles requests that don't match any other endpoints.
    /// </summary>
    /// <param name="pattern">The route pattern for the fallback endpoint.</param>
    /// <param name="handler">The delegate to execute for fallback requests.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapFallback(string pattern, Delegate handler);

    /// <summary>
    /// Maps a fallback endpoint that handles requests that don't match any other endpoints.
    /// </summary>
    /// <param name="handler">The request delegate to execute for fallback requests.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapFallback(RequestDelegate handler);

    /// <summary>
    /// Maps a fallback endpoint with a specific pattern that handles requests that don't match any other endpoints.
    /// </summary>
    /// <param name="pattern">The route pattern for the fallback endpoint.</param>
    /// <param name="handler">The request delegate to execute for fallback requests.</param>
    /// <returns>An IEndpointConventionBuilder for further configuration.</returns>
    IEndpointConventionBuilder MapFallback(string pattern, RequestDelegate handler);

    /// <summary>
    /// Maps a group of endpoints with a common prefix.
    /// This is useful for grouping related endpoints under a common route prefix.
    /// </summary>
    /// <param name="prefix">The pattern that prefixes all routes in this group.</param>
    IHttpEndpointBuilder MapGroup(RoutePattern prefix);

    /// <summary>
    /// Maps a group of endpoints with a common prefix.
    /// This is useful for grouping related endpoints under a common route prefix.
    /// </summary>
    /// <param name="prefix">The pattern that prefixes all routes in this group.</param>
    IHttpEndpointBuilder MapGroup(string prefix);
}
