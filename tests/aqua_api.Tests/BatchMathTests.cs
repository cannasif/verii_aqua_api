using aqua_api.Shared.Common.Helpers;
using Xunit;

namespace aqua_api.Tests;

public class BatchMathTests
{
    [Theory]
    [InlineData(50.000, 0.500, 50.500)]
    [InlineData(1.234, 6.000, 7.234)]
    [InlineData(0.500, 0.001, 0.501)]
    public void CalculateIncrementedAverageGram_ReturnsExpected(decimal currentAverageGram, decimal gramIncrement, decimal expected)
    {
        var result = BatchMath.CalculateIncrementedAverageGram(currentAverageGram, gramIncrement);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(10000, 50.000, 500000.000)]
    [InlineData(3000, 65.000, 195000.000)]
    [InlineData(9850, 50.000, 492500.000)]
    public void CalculateBiomassGram_ReturnsExpected(int fishCount, decimal averageGram, decimal expectedBiomass)
    {
        var result = BatchMath.CalculateBiomassGram(fishCount, averageGram);
        Assert.Equal(expectedBiomass, result);
    }

    [Fact]
    public void ConvertScenario_MatchesExpectedDelta()
    {
        // Scenario: 1000 fish, 1g average, +6g increment => new average 7g, biomass delta 6000g.
        const int fishCount = 1000;
        const decimal fromAverage = 1.000m;
        const decimal increment = 6.000m;

        var toAverage = BatchMath.CalculateIncrementedAverageGram(fromAverage, increment);
        var fromBiomass = BatchMath.CalculateBiomassGram(fishCount, fromAverage);
        var toBiomass = BatchMath.CalculateBiomassGram(fishCount, toAverage);
        var delta = toBiomass - fromBiomass;

        Assert.Equal(7.000m, toAverage);
        Assert.Equal(6000.000m, delta);
    }
}
