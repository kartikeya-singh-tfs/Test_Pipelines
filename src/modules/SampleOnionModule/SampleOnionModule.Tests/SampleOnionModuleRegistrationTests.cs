using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using ThermoFisher.Opal.Shared.Registration.Abstractions;
using ThermoFisher.SampleOnionModule.Abstractions;
using ThermoFisher.SampleOnionModule.Grpc;
using ThermoFisher.SampleOnionModule.Opal;

namespace ThermoFisher.SampleOnionModule.Tests;

public class SampleOnionModuleRegistrationTests
{
    private readonly SampleOnionModuleRegistration _sut;

    public SampleOnionModuleRegistrationTests()
    {
        _sut = new SampleOnionModuleRegistration();
    }

    [Fact]
    public void Name_ShouldReturnCorrectValue()
    {
        Assert.Equal("SampleOnionModule", _sut.Name);
    }

    [Fact]
    public void RegisterServices_ShouldRegisterModuleServices()
    {
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        var environment = Substitute.For<IHostEnvironment>();
        var context = new ModuleRegistrationContext(services, configuration, environment, "test");

        _sut.RegisterServices(context);

        // Assert that all services were registered correctly
        Assert.Contains(services, s => s.ServiceType == typeof(ISampleOnionModule));
        Assert.Contains(services, s => s.ServiceType == typeof(SampleOnionGrpcService));
    }

    [Fact]
    public async Task OnApplicationStartedAsync_WithValidServiceProvider_ShouldCallInitialize()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        var sampleCore = Substitute.For<ISampleOnionCore>();
        serviceProvider.GetService<ISampleOnionCore>().Returns(sampleCore);

        await _sut.OnApplicationStartedAsync(serviceProvider);

        await sampleCore.Received(1).InitializeAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void RegisterOpenApiServices_ShouldReturnDocumentName()
    {
        var openApiRegister = Substitute.For<IOpenApiServiceRegistry>();

        var result = _sut.RegisterOpenApiServices(openApiRegister);

        Assert.Single(result);
        Assert.Contains("sample-onion-v1", result);
    }
}
