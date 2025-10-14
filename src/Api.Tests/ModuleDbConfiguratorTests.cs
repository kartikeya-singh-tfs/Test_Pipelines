using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.DbConnection;
using ThermoFisher.Opal.Api.Tests.TestUtilities;
using ThermoFisher.Opal.Shared.Registration.Abstractions;

namespace ThermoFisher.Opal.Api.Tests;

public class ModuleDbConfiguratorTests
{
    [Fact]
    public void RegisterServices_WithDbRegistrationModules_ShouldRegisterDbServices()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var serviceModule = Substitute.For<IDbRegistrationModule>();
        serviceModule.Name.Returns("TestModule");
        serviceModule.ServiceKey.Returns("ServiceKey");

        builder.RegisterModule(serviceModule);
        var configurator = builder.BuildDbConfigurator();

        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        configuration
            .GetConnectionString("opal")
            .Returns("server=localhost;port=5433;username=opal");
        var environment = TestHelpers.CreateTestEnvironment();

        // Act
        configurator.RegisterServices(services, configuration, environment);

        // Assert
        Assert.Equal(2, services.Count);
        Assert.All<ServiceDescriptor>(
            services,
            sd => Assert.Equal("ServiceKey", sd.ServiceKey as string)
        );
        Assert.Contains<ServiceDescriptor>(
            services,
            sd => sd.ServiceType == typeof(DbDataSource) && sd.Lifetime == ServiceLifetime.Singleton
        );
        Assert.Contains<ServiceDescriptor>(
            services,
            sd => sd.ServiceType == typeof(DbConnection) && sd.Lifetime == ServiceLifetime.Scoped
        );
    }

    [Fact]
    public void RegisterServices_WithNonDbRegistrationModules_ShouldNotRegisterDbServices()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var serviceModule = Substitute.For<IModule>();
        serviceModule.Name.Returns("TestModule");

        builder.RegisterModule(serviceModule);
        var configurator = builder.BuildDbConfigurator();

        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        var environment = TestHelpers.CreateTestEnvironment();

        // Act
        configurator.RegisterServices(services, configuration, environment);

        // Assert
        Assert.Empty(services);
    }

    [Fact]
    public void Validate_DuplicateSchemaName_ShouldThrow()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();

        var serviceModule1 = Substitute.For<IDbRegistrationModule>();
        serviceModule1.Name.Returns("Module1");
        serviceModule1.ServiceKey.Returns("Module1Key");
        serviceModule1.SchemaName.Returns("duplicate");
        builder.RegisterModule(serviceModule1);

        var serviceModule2 = Substitute.For<IDbRegistrationModule>();
        serviceModule2.Name.Returns("Module2");
        serviceModule2.ServiceKey.Returns("Module2Key");
        serviceModule2.SchemaName.Returns("duplicate");
        builder.RegisterModule(serviceModule2);

        var configurator = builder.BuildDbConfigurator();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => configurator.Validate());
    }

    [Fact]
    public void Validate_DuplicateServiceKey_ShouldThrow()
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();

        var serviceModule1 = Substitute.For<IDbRegistrationModule>();
        serviceModule1.Name.Returns("Module1");
        serviceModule1.ServiceKey.Returns("duplicate");
        serviceModule1.SchemaName.Returns("module1");
        builder.RegisterModule(serviceModule1);

        var serviceModule2 = Substitute.For<IDbRegistrationModule>();
        serviceModule2.Name.Returns("Module2");
        serviceModule2.ServiceKey.Returns("duplicate");
        serviceModule2.SchemaName.Returns("module2");
        builder.RegisterModule(serviceModule2);

        var configurator = builder.BuildDbConfigurator();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => configurator.Validate());
    }

    [Theory]
    [InlineData("")]
    [InlineData("unsafe'character")]
    public void RegisterServices_Validate_BadSchemaName_ShouldThrow(string schemaName)
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();

        var serviceModule = Substitute.For<IDbRegistrationModule>();
        serviceModule.Name.Returns("Module");
        serviceModule.ServiceKey.Returns("ServiceKey");
        serviceModule.SchemaName.Returns(schemaName);
        builder.RegisterModule(serviceModule);

        var configurator = builder.BuildDbConfigurator();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => configurator.Validate());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateOrMigrateSchemas_WithDbRegistrationModules_ShouldCallCreateOrMigrateSchema(
        bool userAlreadyExists
    )
    {
        // Arrange
        var builder = TestHelpers.CreateModuleRegistryBuilder();
        var serviceModule = Substitute.For<IDbRegistrationModule>();
        serviceModule.Name.Returns("TestModule");
        serviceModule.SchemaName.Returns("testmodule");

        var createUserWasCalled = false;

        DbConnection dbConnection = Substitute.For<DbConnection>().SetupCommands();
        dbConnection
            .SetupQuery("SELECT COUNT(*) FROM pg_roles WHERE rolname=@name")
            .WithParameter("name", "testmodule")
            .Returns(new { Id = userAlreadyExists ? 1 : 0 });
        dbConnection
            .SetupQuery("CREATE USER testmodule WITH PASSWORD 'testmodule';")
            .Affects(_ =>
            {
                // NSubstitute.DbConnection doesn't support assertions that a query
                // was actually executed, see:
                // https://github.com/jmg48/NSubstitute.DbConnection/issues/19
                // As discussed in that issue, we need to set a flag as a side effect,
                // and check that flag later in an assertion.
                createUserWasCalled = true;
                return 0;
            });
        dbConnection.SetupQuery("CREATE SCHEMA IF NOT EXISTS AUTHORIZATION testmodule;");
        var openConnection = Substitute.For<Func<string, string, string, DbConnection>>();
        openConnection(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(dbConnection);

        var secretStore = Substitute.For<ISecretStore>();
        secretStore.GetDbAdminPassword().Returns("Dummy DB Admin Password");
        secretStore.CreateDbPassword("testmodule").Returns("testmodule");

        builder.RegisterModule(serviceModule);
        var configurator = new ModuleDbConfigurator(
            builder.ModuleRegistrations,
            secretStore,
            openConnection
        );

        var services = TestHelpers.CreateTestServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        configuration
            .GetConnectionString("opal")
            .Returns("server=localhost;port=5433;username=opal");
        var environment = TestHelpers.CreateTestEnvironment();

        // Act
        await configurator.CreateOrMigrateSchemasAsync(
            services,
            configuration,
            environment,
            default
        );

        // Assert
        await serviceModule
            .Received()
            .InitializeOrMigrateSchemaAsync(Arg.Any<DbConnection>(), Arg.Any<CancellationToken>());
        Assert.Equal(!userAlreadyExists, createUserWasCalled);
    }
}
