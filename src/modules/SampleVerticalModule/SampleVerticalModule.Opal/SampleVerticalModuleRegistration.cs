using ThermoFisher.Opal.Shared.Registration.Abstractions;
using ThermoFisher.SampleVerticalModule.Chromatography;
using ThermoFisher.SampleVerticalModule.MassSpectrometry;

namespace ThermoFisher.SampleVerticalModule.Opal;

/// <summary>
/// Registration for Sample Vertical Module
/// This class registers the module with the Opal API host and configures its services.
/// Sample Vertical Module is a sample module that demonstrates how to implement a module with Vertical Slice architecture
/// </summary>
public class SampleVerticalModuleRegistration
    : IServiceRegistrationModule,
        IHttpEndpointRegistrationModule,
        IOpenApiRegistrationModule,
        IMiddlewareRegistrationModule,
        IMessagingRegistrationModule
{
    public string Name => "SampleVerticalModule";

    public void RegisterServices(ModuleRegistrationContext context)
    {
        context.Services.AddChromatography().AddMassSpectrometry();
    }

    public void RegisterEndpoints(IHttpEndpointBuilder endpoints, ModuleStartupContext context)
    {
        endpoints.MapChromatographyEndpoints();
        endpoints.MapMassSpecEndpoints();
    }

    public IReadOnlyCollection<string> RegisterOpenApiServices(
        IOpenApiServiceRegistry openApiServiceRegister
    )
    {
        openApiServiceRegister.AddOpenApi(
            ChromatographyHttpEndpointBuilderExtensions.OpenApiDocumentName,
            options =>
            {
                options.AddDocumentTransformer(
                    (document, context, _) =>
                    {
                        document.Info.Title = "Chromatography API";
                        document.Info.Version = "v1";
                        document.Info.Description = "API for Chromatography Module";
                        return Task.CompletedTask;
                    }
                );
            }
        );

        openApiServiceRegister.AddOpenApi(
            MassSpecHttpEndpointBuilderExtensions.OpenApiDocumentName,
            options =>
            {
                options.AddDocumentTransformer(
                    (document, context, _) =>
                    {
                        document.Info.Title = "Mass Spectrometry API";
                        document.Info.Version = "v1";
                        document.Info.Description = "API for Mass Spectrometry Module";
                        return Task.CompletedTask;
                    }
                );
            }
        );

        return
        [
            ChromatographyHttpEndpointBuilderExtensions.OpenApiDocumentName,
            MassSpecHttpEndpointBuilderExtensions.OpenApiDocumentName,
        ];
    }

    public void RegisterMiddleware(
        IMiddlewareApplicationBuilder appBuilder,
        ModuleStartupContext context
    )
    {
        appBuilder.UseMiddleware<ChromatographyMiddleware>();
    }

    public void RegisterMessageHandlers(
        IMessagingRegistrationBuilder builder,
        ModuleRegistrationContext context
    )
    {
        builder.RegisterMessageHandlers(this.GetType().Assembly);
    }
}
