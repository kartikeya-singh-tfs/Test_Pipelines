using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ThermoFisher.Opal.Api.Tests.TestUtilities;

internal static class TestHelpers
{
    public static IServiceCollection CreateTestServiceCollection()
    {
        return new ServiceCollection();
    }

    public static IConfiguration CreateTestConfiguration(Dictionary<string, string?>? values = null)
    {
        var configBuilder = new ConfigurationBuilder();

        if (values != null)
        {
            configBuilder.AddInMemoryCollection(values);
        }

        return configBuilder.Build();
    }

    public static IHostEnvironment CreateTestEnvironment(string environmentName = "Test")
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(environmentName);
        environment.ApplicationName.Returns("ThermoFisher.Opal.Api.Tests");
        environment.ContentRootPath.Returns(Directory.GetCurrentDirectory());
        return environment;
    }

    public static ModuleRegistryBuilder CreateModuleRegistryBuilder()
    {
        var loggerFactory = LoggerFactory.Create(builder => { });
        return new ModuleRegistryBuilder(loggerFactory);
    }

    public static Dictionary<string, string?> CreateModuleConfiguration(
        string moduleName,
        Dictionary<string, string?> moduleSettings
    )
    {
        var config = new Dictionary<string, string?>();
        foreach (var setting in moduleSettings)
        {
            config[$"Modules:{moduleName}:{setting.Key}"] = setting.Value;
        }
        return config;
    }
}
