using Microsoft.Extensions.DependencyInjection;
using ThermoFisher.AcquisitionModule.Grpc;
using ThermoFisher.Opal.Shared.Registration.Abstractions;

namespace ThermoFisher.AcquisitionModule.Opal
{
    public class AcquisitionModuleRegistration
        : IServiceRegistrationModule,
            IHttpEndpointRegistrationModule,
            IGrpcRegistrationModule,
            IOpenApiRegistrationModule,
            ILifecycleModule
    {
        public string Name => "AcquisitionModule";

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

        public void RegisterEndpoints(IHttpEndpointBuilder endpoints, ModuleStartupContext context)
        {
            endpoints.MapSampleOnionModuleEndpoints();
        }

        public void RegisterGrpcEndpoints(
            IGrpcEndpointBuilder endpoints,
            ModuleStartupContext context
        )
        {
            endpoints.MapGrpcService<AcquisitionService>();
            endpoints.MapGrpcService<RealTimePlotDataAccessService>();
        }

        public IReadOnlyCollection<string> RegisterOpenApiServices(
            IOpenApiServiceRegistry openApiServiceRegister
        )
        {
            openApiServiceRegister.AddOpenApi(
                AcquisitionModuleHttpEndpointBuilderExtensions.OpenApiDocumentName,
                options =>
                {
                    options.AddDocumentTransformer(
                        (document, context, _) =>
                        {
                            document.Info.Title = "Acquisition Module API";
                            document.Info.Version = "v1";
                            document.Info.Description = "API for Acquisition Module";
                            return Task.CompletedTask;
                        }
                    );
                }
            );
            return [AcquisitionModuleHttpEndpointBuilderExtensions.OpenApiDocumentName];
        }

        public void RegisterServices(ModuleRegistrationContext context)
        {
            context.Services.AddSingleton<AcquisitionService>();

            context.Services.AddSingleton<RealTimePlotDataAccessService>();
        }
    }
}
