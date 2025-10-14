using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using ThermoFisher.Opal.Shared.Registration.Abstractions;
using ThermoFisher.SampleOnionModule.Abstractions;
using ThermoFisher.SampleOnionModule.Data.Sql;
using ThermoFisher.SampleOnionModule.Grpc;

namespace ThermoFisher.SampleOnionModule.Opal;

/// <summary>
/// Registration for Sample Onion Module
/// This class registers the module with the Opal API host and configures its services.
/// Sample Onion Module is a sample module that demonstrates how to implement a module with Onion architecture
/// and integrate it in the Opal platform.
/// </summary>
public class SampleOnionModuleRegistration
    : IServiceRegistrationModule,
        IDbRegistrationModule,
        IHttpEndpointRegistrationModule,
        IGrpcRegistrationModule,
        IOpenApiRegistrationModule,
        ILifecycleModule
{
    public string Name => "SampleOnionModule";

    public void RegisterServices(ModuleRegistrationContext context)
    {
        context.Services.AddSampleOnionModule(context.GetModuleConfiguration());
        context.Services.AddTransient<SampleOnionGrpcService>();
        context.Services.AddTransient<ISampleOnionRepository, SampleOnionRepository>();
        context.Services.AddTransient<ITestMessagingService, TestMessagingService>();
    }

    public IReadOnlyCollection<string> RegisterOpenApiServices(
        IOpenApiServiceRegistry openApiServiceRegister
    )
    {
        openApiServiceRegister.AddOpenApi(
            SampleOnionModuleHttpEndpointBuilderExtensions.OpenApiDocumentName,
            options =>
            {
                options.AddDocumentTransformer(
                    (document, context, _) =>
                    {
                        document.Info.Title = "Sample Onion Module API";
                        document.Info.Version = "v1";
                        document.Info.Description = "API for Sample Onion Module";
                        return Task.CompletedTask;
                    }
                );
            }
        );
        return [SampleOnionModuleHttpEndpointBuilderExtensions.OpenApiDocumentName];
    }

    public void RegisterEndpoints(IHttpEndpointBuilder endpoints, ModuleStartupContext context)
    {
        endpoints.MapSampleOnionModuleEndpoints();
    }

    public void RegisterGrpcEndpoints(IGrpcEndpointBuilder endpoints, ModuleStartupContext context)
    {
        endpoints.MapGrpcService<SampleOnionGrpcService>();
    }

    public async Task OnApplicationStartedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    )
    {
        // call ISampleModule.DoSomething when application is started
        var sampleCore = serviceProvider.GetService<ISampleOnionCore>();
        if (sampleCore != null)
        {
            await sampleCore.InitializeAsync(cancellationToken);
        }
        else
        {
            throw new InvalidOperationException("ISampleOnionCore is not registered.");
        }
    }

    public Task OnApplicationStoppedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    )
    {
        // no-op
        return Task.CompletedTask;
    }

    public string ServiceKey => Name;

    public async Task InitializeOrMigrateSchemaAsync(
        DbConnection dbConnection,
        CancellationToken cancellationToken
    )
    {
        await Migrations.InitializeOrMigrateSchemaAsync(dbConnection, cancellationToken);
    }
}
