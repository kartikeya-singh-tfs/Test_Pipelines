using ThermoFisher.SampleVerticalModule.MassSpectrometry;

namespace SampleVerticalModule.Tests;

public class MassSpectrometryTests
{
    [Fact]
    public async Task AnalyzeAsync_ShouldReturnAnalysis()
    {
        var massSpec = new MassSpecAnalysisService();
        const string sampleId = "test-sample";
        const string ionizationMode = "ESI";

        var result = await massSpec.AnalyzeAsync(sampleId, ionizationMode);

        Assert.NotNull(result);
    }
}
