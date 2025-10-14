namespace ThermoFisher.SampleOnionModule.Abstractions;

public interface ISampleOnionCore
{
    public SampleOnionModuleConfiguration Configuration { get; }
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

public interface ISampleOnionModule
{
    IAsyncEnumerable<Compound> GetCompoundsAsync(CancellationToken cancellationToken = default);

    Task<Sample> GetSampleByIdAsync(string id, CancellationToken cancellationToken = default);
    Task CreateSamplesAsync(
        int count,
        string name,
        string? description = default,
        CancellationToken cancellationToken = default
    );
}
