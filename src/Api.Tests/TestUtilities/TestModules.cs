using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ThermoFisher.Opal.Shared.Registration.Abstractions;

namespace ThermoFisher.Opal.Api.Tests.TestUtilities;

internal class TestServiceModule : IModule, IServiceRegistrationModule
{
    public string Name => "TestServiceModule";

    public void RegisterServices(ModuleRegistrationContext context)
    {
        context.Services.AddScoped<ITestService, TestService>();
    }
}

internal class TestHttpEndpointModule : IModule, IHttpEndpointRegistrationModule
{
    public string Name => "TestHttpEndpointModule";

    public void RegisterEndpoints(IHttpEndpointBuilder endpoints, ModuleStartupContext context)
    {
        endpoints.MapGet("/test", () => "Test endpoint");
    }
}

internal class TestOpenApiModule : IModule, IOpenApiRegistrationModule
{
    public string Name => "TestOpenApiModule";

    public IReadOnlyCollection<string> RegisterOpenApiServices(
        IOpenApiServiceRegistry openApiServiceRegister
    )
    {
        openApiServiceRegister.AddOpenApi(
            "test-api-v1",
            options =>
            {
                options.AddDocumentTransformer(
                    (document, context, _) =>
                    {
                        document.Info.Title = "Test API";
                        document.Info.Version = "v1";
                        return Task.CompletedTask;
                    }
                );
            }
        );
        return ["test-api-v1"];
    }
}

internal class TestMiddlewareModule : IModule, IMiddlewareRegistrationModule
{
    public string Name => "TestMiddlewareModule";

    public void RegisterMiddleware(
        IMiddlewareApplicationBuilder appBuilder,
        ModuleStartupContext context
    )
    {
        appBuilder.UseMiddleware<TestMiddleware>();
    }
}

internal class TestLifecycleModule : IModule, ILifecycleModule
{
    public string Name => "TestLifecycleModule";
    public bool StartupCalled { get; private set; }
    public bool ShutdownCalled { get; private set; }

    public Task OnApplicationStartedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    )
    {
        StartupCalled = true;
        return Task.CompletedTask;
    }

    public Task OnApplicationStoppedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    )
    {
        ShutdownCalled = true;
        return Task.CompletedTask;
    }
}

internal class TestFullFeaturedModule
    : IModule,
        IServiceRegistrationModule,
        IHttpEndpointRegistrationModule,
        IGrpcRegistrationModule,
        IOpenApiRegistrationModule,
        IMiddlewareRegistrationModule,
        ILifecycleModule
{
    public string Name => "TestFullFeaturedModule";

    public void RegisterServices(ModuleRegistrationContext context)
    {
        context.Services.AddScoped<ITestService, TestService>();
    }

    public void RegisterEndpoints(IHttpEndpointBuilder endpoints, ModuleStartupContext context)
    {
        endpoints.MapGet("/full-featured", () => "Full featured endpoint");
    }

    public void RegisterGrpcEndpoints(IGrpcEndpointBuilder endpoints, ModuleStartupContext context)
    {
        endpoints.MapGrpcService<TestGrpcService>();
    }

    public IReadOnlyCollection<string> RegisterOpenApiServices(
        IOpenApiServiceRegistry openApiServiceRegister
    )
    {
        openApiServiceRegister.AddOpenApi(
            "full-featured-api-v1",
            options =>
            {
                options.AddDocumentTransformer(
                    (document, context, _) =>
                    {
                        document.Info.Title = "Full Featured API";
                        document.Info.Version = "v1";
                        return Task.CompletedTask;
                    }
                );
            }
        );
        return ["full-featured-api-v1"];
    }

    public void RegisterMiddleware(
        IMiddlewareApplicationBuilder appBuilder,
        ModuleStartupContext context
    )
    {
        appBuilder.UseMiddleware<TestMiddleware>();
    }

    public Task OnApplicationStartedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
    }

    public Task OnApplicationStoppedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
    }
}

internal class TestMinimalModule : IModule
{
    public string Name => "TestMinimalModule";
}

internal class TestMessagingModule : IModule
{
    public string Name => "TestMessagingModule";
}

// Test services and middleware
internal interface ITestService
{
    string GetMessage();
}

internal class TestService : ITestService
{
    public string GetMessage() => "Test service message";
}

internal class TestGrpcService
{
    // Mock gRPC service for testing
}

internal class TestMiddleware
{
    private readonly RequestDelegate _next;

    public TestMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Test-Middleware", "true");
        await _next(context);
    }
}
