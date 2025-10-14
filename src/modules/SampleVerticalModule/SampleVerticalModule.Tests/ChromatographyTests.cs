using ThermoFisher.SampleVerticalModule.Chromatography;

namespace SampleVerticalModule.Tests;

public class ChromatographyTests
{
    [Fact]
    public async Task AnalyzeAsync_ShouldReturnAnalysis()
    {
        var chromatography = new ChromatogramAnalysisService();
        const string sampleId = "test-sample";
        const string methodName = "test-method";

        var result = await chromatography.AnalyzeAsync(sampleId, methodName);
        Assert.NotNull(result);
    }
}
