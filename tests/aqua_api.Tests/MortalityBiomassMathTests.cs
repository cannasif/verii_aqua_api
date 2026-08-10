using aqua_api.Shared.Common.Helpers;
using Xunit;

namespace aqua_api.Tests;

public sealed class MortalityBiomassMathTests
{
    [Fact]
    public void ReportedBiomass_UsesHalfOfCountTimesOperationAverageGram()
    {
        Assert.Equal(1_500m, MortalityBiomassMath.CalculateReportedBiomassGram(10, 300m));
        Assert.Equal(1.5m, MortalityBiomassMath.CalculateReportedBiomassKg(10, 300m));
    }

    [Theory]
    [InlineData(-1, 300, 0)]
    [InlineData(10, -300, 0)]
    [InlineData(0, 300, 0)]
    public void ReportedBiomass_DoesNotReturnNegativeValues(int deadCount, decimal averageGram, decimal expectedKg)
    {
        Assert.Equal(expectedKg, MortalityBiomassMath.CalculateReportedBiomassKg(deadCount, averageGram));
    }
}
