using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using ThermoFisher.Opal.Api.Tests.TestUtilities;
using ThermoFisher.Opal.Shared.Registration.Abstractions;

namespace ThermoFisher.Opal.Api.Tests;

public class ModuleApplicationConfiguratorTests
{
    [Fact]
    public void ConfigureMiddleware_WithMiddlewareModules_ShouldRegisterMiddleware()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var middlewareModule = Substitute.For<IModule, IMiddlewareRegistrationModule>();
        middlewareModule.Name.Returns("TestMiddlewareModule");

        builder.RegisterModule(middlewareModule);
        var serviceConfigurator = builder.BuildServiceConfigurator();
        var appConfigurator = serviceConfigurator.BuildApplicationConfigurator();

        var app = Substitute.For<IApplicationBuilder>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act
        appConfigurator.ConfigureMiddleware(app, serviceProvider, environment);

        // Assert
        ((IMiddlewareRegistrationModule)middlewareModule)
            .Received(1)
            .RegisterMiddleware(
                Arg.Any<IMiddlewareApplicationBuilder>(),
                Arg.Is<ModuleStartupContext>(ctx =>
                    ctx.ServiceProvider == serviceProvider
                    && ctx.ModuleName == "TestMiddlewareModule"
                )
            );
    }

    [Fact]
    public void ConfigureMiddleware_WithNonMiddlewareModules_ShouldSkipModule()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var minimalModule = new TestMinimalModule();

        builder.RegisterModule(minimalModule);
        var serviceConfigurator = builder.BuildServiceConfigurator();
        var appConfigurator = serviceConfigurator.BuildApplicationConfigurator();

        var app = Substitute.For<IApplicationBuilder>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act & Assert
        var exception = Record.Exception(() =>
            appConfigurator.ConfigureMiddleware(app, serviceProvider, environment)
        );
        Assert.Null(exception);
    }

    [Fact]
    public void RegisterEndpoints_WithHttpEndpointModule_ShouldRegisterEndpoints()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var httpEndpointModule = Substitute.For<IModule, IHttpEndpointRegistrationModule>();

        builder.RegisterModule(httpEndpointModule, "test-prefix");
        var serviceConfigurator = builder.BuildServiceConfigurator();
        var appConfigurator = serviceConfigurator.BuildApplicationConfigurator();

        var endpointRouteBuilder = Substitute.For<IEndpointRouteBuilder>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act
        appConfigurator.RegisterEndpoints(endpointRouteBuilder, serviceProvider, environment);

        // Assert
        ((IHttpEndpointRegistrationModule)httpEndpointModule)
            .Received(1)
            .RegisterEndpoints(
                Arg.Any<IHttpEndpointBuilder>(),
                Arg.Is<ModuleStartupContext>(ctx =>
                    ctx.ServiceProvider == serviceProvider && ctx.Environment == environment
                )
            );

        endpointRouteBuilder.Received(1).MapGroup("/api/test-prefix");
    }

    [Fact]
    public void RegisterEndpoints_WithNonHttpEndpointModules_ShouldSkipModule()
    {
        //Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var minimalModule = new TestMinimalModule();

        builder.RegisterModule(minimalModule);
        var serviceConfigurator = builder.BuildServiceConfigurator();
        var appConfigurator = serviceConfigurator.BuildApplicationConfigurator();

        var endpointRouteBuilder = Substitute.For<IEndpointRouteBuilder>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act & Assert
        var exception = Record.Exception(() =>
            appConfigurator.RegisterEndpoints(endpointRouteBuilder, serviceProvider, environment)
        );
        Assert.Null(exception);
    }

    [Fact]
    public void RegisterGrpcEndpoints_WithNonGrpcModules_ShouldSkipModule()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var minimalModule = new TestMinimalModule();

        builder.RegisterModule(minimalModule);
        var serviceConfigurator = builder.BuildServiceConfigurator();
        var appConfigurator = serviceConfigurator.BuildApplicationConfigurator();

        var endpointRouteBuilder = Substitute.For<IEndpointRouteBuilder>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act & Assert
        var exception = Record.Exception(() =>
            appConfigurator.RegisterGrpcEndpoints(
                endpointRouteBuilder,
                serviceProvider,
                environment
            )
        );
        Assert.Null(exception);
    }

    [Fact]
    public void RegisterGrpcEndpoints_WithGrpcModules_ShouldRegisterGrpcService()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var grpcModule = Substitute.For<IModule, IGrpcRegistrationModule>();

        builder.RegisterModule(grpcModule);
        var serviceConfigurator = builder.BuildServiceConfigurator();
        var appConfigurator = serviceConfigurator.BuildApplicationConfigurator();

        var endpointRouteBuilder = Substitute.For<IEndpointRouteBuilder>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act
        appConfigurator.RegisterGrpcEndpoints(endpointRouteBuilder, serviceProvider, environment);

        // Assert
        ((IGrpcRegistrationModule)grpcModule)
            .Received(1)
            .RegisterGrpcEndpoints(
                Arg.Any<IGrpcEndpointBuilder>(),
                Arg.Is<ModuleStartupContext>(ctx =>
                    ctx.ServiceProvider == serviceProvider && ctx.Environment == environment
                )
            );
    }

    [Fact]
    public async Task HandleApplicationEventsAsync_WithLifecycleModules_ShouldCallHandler()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var lifecycleModule = new TestLifecycleModule();

        builder.RegisterModule(lifecycleModule);
        var serviceConfigurator = builder.BuildServiceConfigurator();
        var appConfigurator = serviceConfigurator.BuildApplicationConfigurator();

        var serviceProvider = Substitute.For<IServiceProvider>();
        var handlerCalled = false;

        Func<ILifecycleModule, IServiceProvider, CancellationToken, Task> handler = (
            module,
            sp,
            ct
        ) =>
        {
            handlerCalled = true;
            Assert.Same(lifecycleModule, module);
            Assert.Same(serviceProvider, sp);
            return Task.CompletedTask;
        };

        // Act
        await appConfigurator.HandleApplicationEventsAsync(serviceProvider, handler);

        // Assert
        Assert.True(handlerCalled);
    }

    [Fact]
    public async Task HandleApplicationEventsAsync_WithNonLifecycleModules_ShouldSkipModule()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var minimalModule = new TestMinimalModule();

        builder.RegisterModule(minimalModule);
        var serviceConfigurator = builder.BuildServiceConfigurator();
        var appConfigurator = serviceConfigurator.BuildApplicationConfigurator();

        var serviceProvider = Substitute.For<IServiceProvider>();
        var handlerCallCount = 0;

        Func<ILifecycleModule, IServiceProvider, CancellationToken, Task> handler = (
            module,
            sp,
            ct
        ) =>
        {
            handlerCallCount++;
            return Task.CompletedTask;
        };

        // Act
        await appConfigurator.HandleApplicationEventsAsync(serviceProvider, handler);

        // Assert
        Assert.Equal(0, handlerCallCount);
    }

    [Fact]
    public void Constructor_ShouldAcceptModuleRegistrationsAndOpenApiDocuments()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var openApiModule = new TestOpenApiModule();

        builder.RegisterModule(openApiModule);
        var serviceConfigurator = builder.BuildServiceConfigurator();

        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = TestHelpers.CreateTestConfiguration();
        var environment = TestHelpers.CreateTestEnvironment();

        serviceConfigurator.ConfigureOpenApiServices(services, configuration, environment);

        // Act
        var appConfigurator = serviceConfigurator.BuildApplicationConfigurator();

        // Assert
        Assert.NotNull(appConfigurator);
        Assert.True(appConfigurator.GetOpenApiDocuments().ContainsKey(openApiModule));
    }
}
