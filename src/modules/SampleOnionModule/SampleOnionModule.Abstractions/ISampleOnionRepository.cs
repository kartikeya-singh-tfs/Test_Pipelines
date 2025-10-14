namespace ThermoFisher.SampleOnionModule.Abstractions;

public interface ISampleOnionRepository
{
    Task<Sample> GetSampleByIdAsync(string id, CancellationToken cancellationToken = default);
    Task AddSamplesAsync(IEnumerable<Sample> samples);
}
