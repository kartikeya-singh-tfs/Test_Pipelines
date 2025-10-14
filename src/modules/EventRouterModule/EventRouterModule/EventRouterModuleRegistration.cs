using Microsoft.Extensions.DependencyInjection;
using ThermoFisher.Opal.Shared.Registration.Abstractions;

namespace ThermoFisher.EventRouterModule;

/// <summary>
/// Registers the event router module with the application.
/// </summary>
public class EventRouterModuleRegistration
    : IServiceRegistrationModule,
        IHttpEndpointRegistrationModule,
        IOpenApiRegistrationModule,
        IMessagingRegistrationModule
{
    /// <inheritdoc/>
    public string Name => "EventRouterModule";

    /// <inheritdoc/>
    public void RegisterEndpoints(IHttpEndpointBuilder endpoints, ModuleStartupContext context)
    {
        endpoints.MapEventRouterModuleEndpoints();
    }

    /// <inheritdoc/>
    public void RegisterMessageHandlers(
        IMessagingRegistrationBuilder builder,
        ModuleRegistrationContext context
    )
    {
        builder.RegisterMessageHandlers<EventRouterMessageHandler>();
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<string> RegisterOpenApiServices(
        IOpenApiServiceRegistry openApiServiceRegister
    )
    {
        openApiServiceRegister.AddOpenApi(
            EventRouterModuleHttpEndpointBuilderExtensions.OpenApiDocumentName,
            options =>
            {
                options.AddDocumentTransformer(
                    (document, context, _) =>
                    {
                        document.Info.Title = "Event Router Module API";
                        document.Info.Version = "v1";
                        document.Info.Description = "API for Event Router Module";
                        return Task.CompletedTask;
                    }
                );
            }
        );
        return [EventRouterModuleHttpEndpointBuilderExtensions.OpenApiDocumentName];
    }

    /// <inheritdoc/>
    public void RegisterServices(ModuleRegistrationContext context)
    {
        context.Services.AddSingleton<IEventRouterService, EventRouterService>();
        context.Services.AddSingleton<ITopicMatcherService, TopicMatcherService>();
    }
}
