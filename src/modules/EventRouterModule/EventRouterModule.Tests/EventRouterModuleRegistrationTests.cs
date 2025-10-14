using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using ThermoFisher.EventRouterModule;
using ThermoFisher.Opal.Shared.Registration.Abstractions;

namespace EventRouterModule.Tests;

public class EventRouterModuleRegistrationTests
{
    private readonly EventRouterModuleRegistration _module = new();
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        Assert.Equal("EventRouterModule", _module.Name);
    }

    [Fact]
    public void RegisterMessageHandlers_RegistersSseMessageHandler()
    {
        var builder = Substitute.For<IMessagingRegistrationBuilder>();
        var context = Substitute.For<ModuleRegistrationContext>(
            Substitute.For<IServiceCollection>(),
            Substitute.For<IConfiguration>(),
            Substitute.For<IHostEnvironment>(),
            "SseModule"
        );

        _module.RegisterMessageHandlers(builder, context);

        builder.Received(1).RegisterMessageHandlers<EventRouterMessageHandler>();
    }
}
