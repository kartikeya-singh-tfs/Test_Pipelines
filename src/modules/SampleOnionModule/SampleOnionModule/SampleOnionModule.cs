using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using ThermoFisher.SampleOnionModule.Abstractions;

namespace ThermoFisher.SampleOnionModule;

internal class SampleOnionCore(IOptions<SampleOnionModuleConfiguration> configuration)
    : ISampleOnionCore
{
    public SampleOnionModuleConfiguration Configuration =>
        configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // No-op
        return Task.CompletedTask;
    }
}

internal class SampleOnionModule(ISampleOnionCore core, ISampleOnionRepository repository)
    : ISampleOnionModule
{
    public async IAsyncEnumerable<Compound> GetCompoundsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        // Simulate asynchronous data retrieval
        await Task.Delay(core.Configuration.Option2, cancellationToken); // Simulating some delay

        yield return new Compound(
            "Compound A",
            "C6H12O6",
            "Glucose",
            1500.0,
            5.2,
            0.75,
            new[] { "sugar", "carbohydrate" },
            90
        );
        yield return new Compound(
            "Compound B",
            "C2H5OH",
            "Ethanol",
            1200.0,
            3.8,
            0.50,
            new[] { "alcohol", "solvent" },
            85
        );
        yield return new Compound(
            "Compound C",
            "C3H8O",
            "Isopropanol",
            800.0,
            4.5,
            0.60,
            new[] { "cleaner", "disinfectant" },
            80
        );
        yield return new Compound(
            "Compound D",
            "C4H10O",
            "Butanol",
            600.0,
            6.1,
            0.40,
            new[] { "solvent", "fuel" },
            75
        );
        yield return new Compound(
            "Compound E",
            "C5H12O",
            "Pentanol",
            400.0,
            7.3,
            0.30,
            new[] { "fragrance", "solvent" },
            70
        );
    }

    public Task<Sample> GetSampleByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return repository.GetSampleByIdAsync(id, cancellationToken);
    }

    public async Task CreateSamplesAsync(
        int count,
        string name,
        string? description = null,
        CancellationToken cancellationToken = default
    )
    {
        var now = DateTime.UtcNow;
        var samples = Enumerable
            .Range(0, count)
            .Select(index => new Sample(
                "dummy",
                $@"{name}-{index}",
                description ?? String.Empty,
                now,
                now,
                []
            ));
        await repository.AddSamplesAsync(samples);
    }
}
