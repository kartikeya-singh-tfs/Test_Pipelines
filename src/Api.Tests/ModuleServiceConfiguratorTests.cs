using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ThermoFisher.Opal.Api.Tests.TestUtilities;
using ThermoFisher.Opal.Shared.Registration.Abstractions;
using Wolverine;

namespace ThermoFisher.Opal.Api.Tests;

public class ModuleServiceConfiguratorTests
{
    [Fact]
    public void ConfigureServices_WithServiceRegistrationModules_ShouldCallRegisterServices()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var serviceModule = Substitute.For<IModule, IServiceRegistrationModule>();
        serviceModule.Name.Returns("TestModule");

        builder.RegisterModule(serviceModule);
        var configurator = builder.BuildServiceConfigurator();

        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = TestHelpers.CreateTestConfiguration();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act
        configurator.ConfigureServices(services, configuration, environment);

        // Assert
        ((IServiceRegistrationModule)serviceModule)
            .Received(1)
            .RegisterServices(
                Arg.Is<ModuleRegistrationContext>(ctx =>
                    ctx.Services == services && ctx.ModuleName == "TestModule"
                )
            );
    }

    [Fact]
    public void ConfigureServices_WithNonServiceRegistrationModules_ShouldSkipModule()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var minimalModule = new TestMinimalModule();

        builder.RegisterModule(minimalModule);
        var configurator = builder.BuildServiceConfigurator();

        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = TestHelpers.CreateTestConfiguration();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act & Assert
        var exception = Record.Exception(() =>
            configurator.ConfigureServices(services, configuration, environment)
        );
        Assert.Null(exception);
    }

    [Fact]
    public void ConfigureMessaging_WithMessagingRegistrationModules_ShouldCallRegisterMessageHandlers()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var messagingModule = Substitute.For<IModule, IMessagingRegistrationModule>();
        messagingModule.Name.Returns("MessagingModule");

        builder.RegisterModule(messagingModule);
        var configurator = builder.BuildMessagingConfigurator();

        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = TestHelpers.CreateTestConfiguration();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act
        configurator.ConfigureMessaging(services, configuration, environment);

        // Assert
        // The AddWolverine extension should have been called and the module's RegisterMessageHandlers should be invoked
        ((IMessagingRegistrationModule)messagingModule)
            .Received(1)
            .RegisterMessageHandlers(
                Arg.Any<IMessagingRegistrationBuilder>(),
                Arg.Is<ModuleRegistrationContext>(ctx =>
                    ctx.Services == services && ctx.ModuleName == "MessagingModule"
                )
            );
    }

    [Fact]
    public void ConfigureMessaging_WithNonMessagingModules_ShouldSkipModule()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var minimalModule = new TestMinimalModule();

        builder.RegisterModule(minimalModule);
        var configurator = builder.BuildMessagingConfigurator();

        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = TestHelpers.CreateTestConfiguration();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act & Assert
        var exception = Record.Exception(() =>
            configurator.ConfigureMessaging(services, configuration, environment)
        );
        Assert.Null(exception);
    }

    [Fact]
    public void ConfigureMessaging_RegistersIMessageBus()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var messagingModule = new TestMessagingModule();

        builder.RegisterModule(messagingModule);
        var configurator = builder.BuildMessagingConfigurator();

        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = TestHelpers.CreateTestConfiguration();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act
        configurator.ConfigureMessaging(services, configuration, environment);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var messenger = serviceProvider.GetService<IMessageBus>();
        Assert.NotNull(messenger);
    }

    [Fact]
    public void ConfigureServices_WithMultipleModules_ShouldCallAllServiceModules()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var serviceModule = new TestServiceModule();
        var fullFeaturedModule = new TestFullFeaturedModule();

        builder.RegisterModule(serviceModule);
        builder.RegisterModule(fullFeaturedModule, "full-featured");

        var serviceConfigurator = builder.BuildServiceConfigurator();
        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = TestHelpers.CreateTestConfiguration();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act
        serviceConfigurator.ConfigureServices(services, configuration, environment);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Both modules register ITestService, so we should have multiple registrations
        var testServices = serviceProvider.GetServices<ITestService>().ToList();
        Assert.Equal(2, testServices.Count);
        Assert.True(testServices.All(s => s.GetMessage().Contains("Test service")));
    }

    [Fact]
    public void ConfigureOpenApiServices_WithOpenApiModules_ShouldRegisterOpenApiServices()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var openApiModule = new TestOpenApiModule();

        builder.RegisterModule(openApiModule);
        var configurator = builder.BuildServiceConfigurator();

        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = TestHelpers.CreateTestConfiguration();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act
        configurator.ConfigureOpenApiServices(services, configuration, environment);

        // Assert
        // Verify that OpenAPI services were added to the service collection
        Assert.True(services.Count > 0);
    }

    [Fact]
    public void ConfigureOpenApiServices_WithNonOpenApiModules_ShouldSkipModule()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var minimalModule = new TestMinimalModule();

        builder.RegisterModule(minimalModule);
        var configurator = builder.BuildServiceConfigurator();

        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = TestHelpers.CreateTestConfiguration();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act & Assert
        var exception = Record.Exception(() =>
            configurator.ConfigureOpenApiServices(services, configuration, environment)
        );
        Assert.Null(exception);
        Assert.Empty(services);
    }

    [Fact]
    public void ConfigureOpenApiServices_ShouldTrackOpenApiDocuments()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var openApiModule = new TestOpenApiModule();

        builder.RegisterModule(openApiModule);
        var configurator = builder.BuildServiceConfigurator();

        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = TestHelpers.CreateTestConfiguration();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act
        configurator.ConfigureOpenApiServices(services, configuration, environment);
        var appConfigurator = configurator.BuildApplicationConfigurator();

        // Assert
        var openApiDocuments = appConfigurator.GetOpenApiDocuments();
        Assert.True(openApiDocuments.ContainsKey(openApiModule));
        Assert.Contains("test-api-v1", openApiDocuments[openApiModule]);
    }

    [Fact]
    public void ConfigureServices_AfterBuildApplicationConfigurator_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        builder.RegisterModule(new TestServiceModule());
        var configurator = builder.BuildServiceConfigurator();

        configurator.BuildApplicationConfigurator();

        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = TestHelpers.CreateTestConfiguration();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            configurator.ConfigureServices(services, configuration, environment)
        );
    }

    [Fact]
    public void ConfigureOpenApiServices_AfterBuildApplicationConfigurator_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        builder.RegisterModule(new TestOpenApiModule());
        var configurator = builder.BuildServiceConfigurator();

        configurator.BuildApplicationConfigurator();

        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = TestHelpers.CreateTestConfiguration();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            configurator.ConfigureOpenApiServices(services, configuration, environment)
        );
    }

    [Fact]
    public void BuildApplicationConfigurator_ShouldReturnValidConfigurator()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        builder.RegisterModule(new TestMinimalModule());
        var configurator = builder.BuildServiceConfigurator();

        // Act
        var appConfigurator = configurator.BuildApplicationConfigurator();

        // Assert
        Assert.NotNull(appConfigurator);
    }
}
