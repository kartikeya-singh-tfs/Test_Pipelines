namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Interface for modules that need to configure the request pipeline
/// </summary>
public interface IMiddlewareRegistrationModule : IModule
{
    /// <summary>
    /// Registers middleware components for this module in the request pipeline.
    /// </summary>
    /// <param name="appBuilder">The middleware application builder used to register middleware components.</param>
    /// <param name="context">The module startup context containing service provider and environment information.</param>
    void RegisterMiddleware(IMiddlewareApplicationBuilder appBuilder, ModuleStartupContext context);
}

/// <summary>
/// Interface for building middleware pipeline in modules.
/// </summary>
public interface IMiddlewareApplicationBuilder
{
    /// <summary>
    /// Adds a middleware to the request pipeline.
    /// </summary>
    /// <param name="middlewareType">The type of the middleware to add.</param>
    /// <param name="args">Optional arguments to pass to the middleware constructor.</param>
    IMiddlewareApplicationBuilder UseMiddleware(Type middlewareType, params object?[] args);

    /// <summary>
    /// Adds a middleware to the request pipeline.
    /// </summary>
    /// <param name="TMiddleware">The type of the middleware to add.</param>
    /// <param name="args">Optional arguments to pass to the middleware constructor.</param>
    IMiddlewareApplicationBuilder UseMiddleware<TMiddleware>(params object?[] args);
}
