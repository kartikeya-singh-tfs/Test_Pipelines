using Microsoft.AspNetCore.Routing.Patterns;
using ThermoFisher.Opal.Shared.Registration.Abstractions;

namespace ThermoFisher.Opal.Api;

/// <summary>
/// Phase 3 of the module registration process - Application Configuration.
/// This class handles configuring middleware, HTTP endpoints, gRPC endpoints, and lifecycle events
/// for all registered modules. This is the final phase where the application pipeline is configured
/// and modules are integrated into the running application.
/// </summary>
internal partial class ModuleApplicationConfigurator
{
    private readonly IReadOnlyList<ModuleRegistrationEntry> _moduleRegistrations;
    private readonly IReadOnlyDictionary<IModule, IReadOnlyCollection<string>> _openApiDocuments;
    private readonly ILogger<ModuleApplicationConfigurator> _logger;

    private const string ApiRouteBase = "api";

    /// <summary>
    /// Initializes a new instance of ModuleApplicationConfigurator with registered modules and OpenAPI documents.
    /// This constructor is internal and should only be called by ModuleServiceConfigurator.
    /// </summary>
    /// <param name="moduleRegistrations">List of module registrations from previous phases.</param>
    /// <param name="openApiDocuments">Dictionary of modules to their registered OpenAPI document names.</param>
    /// <param name="logger">Logger instance for capturing application configuration flow.</param>
    internal ModuleApplicationConfigurator(
        IReadOnlyList<ModuleRegistrationEntry> moduleRegistrations,
        IReadOnlyDictionary<IModule, IReadOnlyCollection<string>> openApiDocuments,
        ILogger<ModuleApplicationConfigurator> logger
    )
    {
        _moduleRegistrations = moduleRegistrations;
        _openApiDocuments = openApiDocuments;
        _logger = logger;
    }

    /// <summary>
    /// Gets the OpenAPI documents registered by modules during the service configuration phase.
    /// </summary>
    /// <returns>A dictionary mapping modules to their registered OpenAPI document names.</returns>
    public IReadOnlyDictionary<IModule, IReadOnlyCollection<string>> GetOpenApiDocuments()
    {
        return _openApiDocuments;
    }

    /// <summary>
    /// Configures middleware for all registered modules that implement IMiddlewareRegistrationModule.
    /// Middleware is registered in the order modules were registered and executes in the same order.
    /// </summary>
    /// <param name="app">The application builder to configure middleware with.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <param name="environment">The hosting environment information.</param>
    public void ConfigureMiddleware(
        IApplicationBuilder app,
        IServiceProvider serviceProvider,
        IHostEnvironment environment
    )
    {
        LogStartingMiddlewareConfiguration();
        var configuredCount = 0;

        foreach (var registration in _moduleRegistrations)
        {
            if (registration.Module is IMiddlewareRegistrationModule middlewareModule)
            {
                LogConfiguringMiddlewareForModule(middlewareModule.Name);

                var context = new ModuleStartupContext(
                    serviceProvider,
                    environment,
                    middlewareModule.Name
                );
                middlewareModule.RegisterMiddleware(new MiddlewareApplicationBuilder(app), context);
                configuredCount++;

                LogSuccessfullyConfiguredMiddlewareForModule(middlewareModule.Name);
            }
        }

        LogMiddlewareConfigurationCompleted(configuredCount);
    }

    /// <summary>
    /// Internal implementation of IMiddlewareApplicationBuilder that wraps the application builder
    /// to provide middleware registration capabilities to modules.
    /// </summary>
    private class MiddlewareApplicationBuilder : IMiddlewareApplicationBuilder
    {
        private readonly IApplicationBuilder _app;

        public MiddlewareApplicationBuilder(IApplicationBuilder app)
        {
            _app = app;
        }

        public IMiddlewareApplicationBuilder UseMiddleware(
            Type middlewareType,
            params object?[] args
        )
        {
            _app.UseMiddleware(middlewareType, args);
            return this;
        }

        public IMiddlewareApplicationBuilder UseMiddleware<TMiddleware>(params object?[] args)
        {
            _app.UseMiddleware<TMiddleware>(args);
            return this;
        }
    }

    /// <summary>
    /// Handles application lifecycle events for all registered modules that implement ILifecycleModule.
    /// This method is typically called by the ModuleLifecycleService for application startup and shutdown events.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <param name="handler">The handler function to call for each lifecycle module.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleApplicationEventsAsync(
        IServiceProvider serviceProvider,
        Func<ILifecycleModule, IServiceProvider, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var registration in _moduleRegistrations)
        {
            if (registration.Module is ILifecycleModule lifecycleModule)
            {
                await handler(lifecycleModule, serviceProvider, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Registers gRPC endpoints for all registered modules that implement IGrpcRegistrationModule.
    /// Each module can register multiple gRPC services through the provided endpoint builder.
    /// </summary>
    /// <param name="endpointRouteBuilder">The endpoint route builder to configure gRPC endpoints with.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <param name="environment">The hosting environment information.</param>
    public void RegisterGrpcEndpoints(
        IEndpointRouteBuilder endpointRouteBuilder,
        IServiceProvider serviceProvider,
        IHostEnvironment environment
    )
    {
        LogStartingGrpcEndpointRegistration();
        var registeredCount = 0;

        foreach (var registration in _moduleRegistrations)
        {
            if (registration.Module is IGrpcRegistrationModule grpcModule)
            {
                LogRegisteringGrpcEndpointsForModule(grpcModule.Name);

                var context = new ModuleStartupContext(
                    serviceProvider,
                    environment,
                    grpcModule.Name
                );
                grpcModule.RegisterGrpcEndpoints(
                    new GrpcEndpointBuilder(endpointRouteBuilder),
                    context
                );
                registeredCount++;

                LogSuccessfullyRegisteredGrpcEndpointsForModule(grpcModule.Name);
            }
        }

        LogGrpcEndpointRegistrationCompleted(registeredCount);
    }

    /// <summary>
    /// Internal implementation of IGrpcEndpointBuilder that wraps the endpoint route builder
    /// to provide gRPC endpoint registration capabilities to modules.
    /// </summary>
    private class GrpcEndpointBuilder : IGrpcEndpointBuilder
    {
        private readonly IEndpointRouteBuilder _endpoints;

        public GrpcEndpointBuilder(IEndpointRouteBuilder endpoints)
        {
            _endpoints = endpoints;
        }

        public IEndpointConventionBuilder MapGrpcService<TService>()
            where TService : class
        {
            return _endpoints.MapGrpcService<TService>();
        }
    }

    /// <summary>
    /// Registers HTTP REST API endpoints for all registered modules that implement IHttpEndpointRegistrationModule.
    /// Each module's endpoints are grouped under their custom path prefix (e.g., /api/{pathPrefix}/).
    /// </summary>
    /// <param name="endpointRoutBuilder">The endpoint route builder to configure HTTP endpoints with.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <param name="environment">The hosting environment information.</param>
    public void RegisterEndpoints(
        IEndpointRouteBuilder endpointRoutBuilder,
        IServiceProvider serviceProvider,
        IHostEnvironment environment
    )
    {
        LogStartingHttpEndpointRegistration();
        var registeredCount = 0;

        foreach (var registration in _moduleRegistrations)
        {
            if (registration.Module is IHttpEndpointRegistrationModule endpointModule)
            {
                var apiPath = $"{ApiRouteBase}/{registration.PathPrefix}";
                LogRegisteringHttpEndpointsForModule(endpointModule.Name, apiPath);

                var context = new ModuleStartupContext(
                    serviceProvider,
                    environment,
                    endpointModule.Name
                );

                var moduleApiGroup = endpointRoutBuilder.MapGroup(apiPath);
                endpointModule.RegisterEndpoints(new HttpEndpointBuilder(moduleApiGroup), context);
                registeredCount++;

                LogSuccessfullyRegisteredHttpEndpointsForModule(endpointModule.Name);
            }
        }

        LogHttpEndpointRegistrationCompleted(registeredCount);
    }

    /// <summary>
    /// Internal implementation of IHttpEndpointBuilder that wraps a RouteGroupBuilder
    /// to provide HTTP endpoint registration capabilities to modules.
    /// This implementation delegates all endpoint registration calls to the underlying RouteGroupBuilder.
    /// </summary>
    private class HttpEndpointBuilder : IHttpEndpointBuilder
    {
        private readonly RouteGroupBuilder _endpoints;

        public HttpEndpointBuilder(RouteGroupBuilder endpoints)
        {
            _endpoints = endpoints;
        }

        public void Add(Action<EndpointBuilder> convention)
        {
            ((IEndpointConventionBuilder)_endpoints).Add(convention);
        }

        public IEndpointConventionBuilder Map(RoutePattern pattern, RequestDelegate requestDelegate)
        {
            return _endpoints.Map(pattern, requestDelegate);
        }

        public IEndpointConventionBuilder Map(RoutePattern pattern, Delegate requestDelegate)
        {
            return _endpoints.Map(pattern, requestDelegate);
        }

        public IEndpointConventionBuilder Map(string pattern, RequestDelegate requestDelegate)
        {
            return _endpoints.Map(pattern, requestDelegate);
        }

        public IEndpointConventionBuilder Map(string pattern, Delegate requestDelegate)
        {
            return _endpoints.Map(pattern, requestDelegate);
        }

        public IEndpointConventionBuilder MapDelete(string pattern, Delegate handler)
        {
            return _endpoints.MapDelete(pattern, handler);
        }

        public IEndpointConventionBuilder MapDelete(string pattern, RequestDelegate handler)
        {
            return _endpoints.MapDelete(pattern, handler);
        }

        public IEndpointConventionBuilder MapFallback(Delegate handler)
        {
            return _endpoints.MapFallback(handler);
        }

        public IEndpointConventionBuilder MapFallback(string pattern, Delegate handler)
        {
            return _endpoints.MapFallback(pattern, handler);
        }

        public IEndpointConventionBuilder MapFallback(RequestDelegate handler)
        {
            return _endpoints.MapFallback(handler);
        }

        public IEndpointConventionBuilder MapFallback(string pattern, RequestDelegate handler)
        {
            return _endpoints.MapFallback(pattern, handler);
        }

        public IEndpointConventionBuilder MapGet(string pattern, Delegate handler)
        {
            return _endpoints.MapGet(pattern, handler);
        }

        public IEndpointConventionBuilder MapGet(string pattern, RequestDelegate handler)
        {
            return _endpoints.MapGet(pattern, handler);
        }

        public IHttpEndpointBuilder MapGroup(RoutePattern prefix)
        {
            var group = _endpoints.MapGroup(prefix);
            return new HttpEndpointBuilder(group);
        }

        public IHttpEndpointBuilder MapGroup(string prefix)
        {
            var group = _endpoints.MapGroup(prefix);
            return new HttpEndpointBuilder(group);
        }

        public IEndpointConventionBuilder MapMethods(
            string pattern,
            IEnumerable<string> httpMethods,
            RequestDelegate requestDelegate
        )
        {
            return _endpoints.MapMethods(pattern, httpMethods, requestDelegate);
        }

        public IEndpointConventionBuilder MapMethods(
            string pattern,
            IEnumerable<string> httpMethods,
            Delegate requestDelegate
        )
        {
            return _endpoints.MapMethods(pattern, httpMethods, requestDelegate);
        }

        public IEndpointConventionBuilder MapPatch(string pattern, Delegate handler)
        {
            return _endpoints.MapPatch(pattern, handler);
        }

        public IEndpointConventionBuilder MapPatch(string pattern, RequestDelegate handler)
        {
            return _endpoints.MapPatch(pattern, handler);
        }

        public IEndpointConventionBuilder MapPost(string pattern, Delegate handler)
        {
            return _endpoints.MapPost(pattern, handler);
        }

        public IEndpointConventionBuilder MapPost(string pattern, RequestDelegate handler)
        {
            return _endpoints.MapPost(pattern, handler);
        }

        public IEndpointConventionBuilder MapPut(string pattern, Delegate handler)
        {
            return _endpoints.MapPut(pattern, handler);
        }

        public IEndpointConventionBuilder MapPut(string pattern, RequestDelegate handler)
        {
            return _endpoints.MapPut(pattern, handler);
        }
    }

    #region Source-Generated Logging Methods

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Starting middleware configuration for modules"
    )]
    private partial void LogStartingMiddlewareConfiguration();

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Configuring middleware for module: {ModuleName}"
    )]
    private partial void LogConfiguringMiddlewareForModule(string moduleName);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Successfully configured middleware for module: {ModuleName}"
    )]
    private partial void LogSuccessfullyConfiguredMiddlewareForModule(string moduleName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Middleware configuration completed. {ConfiguredCount} modules configured middleware"
    )]
    private partial void LogMiddlewareConfigurationCompleted(int configuredCount);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Starting gRPC endpoint registration for modules"
    )]
    private partial void LogStartingGrpcEndpointRegistration();

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Registering gRPC endpoints for module: {ModuleName}"
    )]
    private partial void LogRegisteringGrpcEndpointsForModule(string moduleName);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Successfully registered gRPC endpoints for module: {ModuleName}"
    )]
    private partial void LogSuccessfullyRegisteredGrpcEndpointsForModule(string moduleName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "gRPC endpoint registration completed. {RegisteredCount} modules registered endpoints"
    )]
    private partial void LogGrpcEndpointRegistrationCompleted(int registeredCount);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Starting HTTP endpoint registration for modules"
    )]
    private partial void LogStartingHttpEndpointRegistration();

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Registering HTTP endpoints for module: {ModuleName} at path: /{ApiPath}"
    )]
    private partial void LogRegisteringHttpEndpointsForModule(string moduleName, string apiPath);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Successfully registered HTTP endpoints for module: {ModuleName}"
    )]
    private partial void LogSuccessfullyRegisteredHttpEndpointsForModule(string moduleName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "HTTP endpoint registration completed. {RegisteredCount} modules registered endpoints"
    )]
    private partial void LogHttpEndpointRegistrationCompleted(int registeredCount);

    #endregion
}
