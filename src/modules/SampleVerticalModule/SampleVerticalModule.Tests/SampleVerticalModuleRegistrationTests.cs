using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using ThermoFisher.Opal.Shared.Registration.Abstractions;
using ThermoFisher.SampleVerticalModule.Chromatography.Abstractions;
using ThermoFisher.SampleVerticalModule.MassSpectrometry.Abstractions;
using ThermoFisher.SampleVerticalModule.Opal;

namespace SampleVerticalModule.Tests;

public class SampleVerticalModuleRegistrationTests
{
    private readonly SampleVerticalModuleRegistration _sut;

    public SampleVerticalModuleRegistrationTests()
    {
        _sut = new SampleVerticalModuleRegistration();
    }

    [Fact]
    public void Name_ShouldReturnCorrectValue()
    {
        Assert.Equal("SampleVerticalModule", _sut.Name);
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
        Assert.Contains(services, s => s.ServiceType == typeof(IMassSpectrometryAnalysis));
        Assert.Contains(services, s => s.ServiceType == typeof(IChromatogramAnalysis));
    }

    [Fact]
    public void RegisterOpenApiServices_ShouldReturnTwoDocuments()
    {
        var openApiRegister = Substitute.For<IOpenApiServiceRegistry>();

        var result = _sut.RegisterOpenApiServices(openApiRegister);

        Assert.Equal(2, result.Count);
        Assert.Contains("chromatogram-analysis-v1", result);
        Assert.Contains("mass-spec-v1", result);
    }
}
