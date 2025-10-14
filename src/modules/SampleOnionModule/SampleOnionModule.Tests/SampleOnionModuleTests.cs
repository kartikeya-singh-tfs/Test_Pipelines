using Microsoft.Extensions.Options;
using NSubstitute;
using ThermoFisher.SampleOnionModule.Abstractions;

namespace ThermoFisher.SampleOnionModule.Tests;

public class SampleOnionModuleTests
{
    [Fact]
    public async Task GetSampleByIdAsync_ShouldReturnCorrectSample()
    {
        // Arrange
        IOptions<SampleOnionModuleConfiguration> options = Options.Create(
            new SampleOnionModuleConfiguration { Option1 = "http://localhost:5000", Option2 = 42 }
        );
        var core = new SampleOnionCore(options);
        const string testId = "test-sample-id";
        var fakeSample = new Sample(
            testId,
            "Test",
            "",
            DateTime.Now,
            DateTime.Now,
            new List<string>()
        );
        ISampleOnionRepository repository = Substitute.For<ISampleOnionRepository>();
        repository.GetSampleByIdAsync(testId).Returns(Task.FromResult(fakeSample));
        var sampleService = new SampleOnionModule(core, repository);

        // Act
        var result = await sampleService.GetSampleByIdAsync(testId);

        // Assert
        Assert.Equal(testId, result.Id);
    }

    [Fact]
    public async Task CreateSamplesAsync_ShouldCreateCorrectSamples()
    {
        // Arrange
        IOptions<SampleOnionModuleConfiguration> options = Options.Create(
            new SampleOnionModuleConfiguration { Option1 = "http://localhost:5000", Option2 = 42 }
        );
        var core = new SampleOnionCore(options);

        ISampleOnionRepository repository = Substitute.For<ISampleOnionRepository>();
        var sampleService = new SampleOnionModule(core, repository);

        // Act
        await sampleService.CreateSamplesAsync(3, "Test", "description");

        // Assert
        await repository
            .Received()
            .AddSamplesAsync(Arg.Is<IEnumerable<Sample>>(l => l.Count() == 3));
    }
}
