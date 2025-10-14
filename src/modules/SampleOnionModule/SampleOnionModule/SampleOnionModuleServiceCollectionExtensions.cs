using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThermoFisher.SampleOnionModule.Abstractions;

namespace ThermoFisher.SampleOnionModule;

public static class SampleOnionModuleServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Sample Onion module to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the module to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSampleOnionModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<SampleOnionModuleConfiguration>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ISampleOnionCore, SampleOnionCore>();
        services.AddScoped<ISampleOnionModule, SampleOnionModule>();

        return services;
    }
}
