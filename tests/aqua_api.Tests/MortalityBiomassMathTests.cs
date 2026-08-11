using aqua_api.Shared.Common.Helpers;
using aqua_api.Modules.Aqua.Application.Services;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.AquaSettings.Domain.Enums;
using aqua_api.Modules.BatchBalances.Domain.Entities;
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

    [Fact]
    public void ReportCalculator_UsesConfiguredHistoricalOrPeriodEndWeight()
    {
        var movements = new List<BatchMovement>
        {
            Movement(1, 10, new DateTime(2026, 1, 1), BatchMovementType.Stocking, 100, 100m),
            Movement(1, 10, new DateTime(2026, 3, 1), BatchMovementType.FishGrowth, 0, 300m),
        };

        var historical = MortalityReportBiomassCalculator.CalculateReportedBiomassGram(
            MortalityBiomassCalculationMode.HistoricalEventWeight,
            deadCount: 20,
            historicalActualBiomassGram: 3_000m,
            movements,
            fishBatchId: 1,
            projectCageId: 10,
            periodEnd: new DateTime(2026, 3, 31));
        var periodEnd = MortalityReportBiomassCalculator.CalculateReportedBiomassGram(
            MortalityBiomassCalculationMode.PeriodEndLatestWeight,
            deadCount: 20,
            historicalActualBiomassGram: 3_000m,
            movements,
            fishBatchId: 1,
            projectCageId: 10,
            periodEnd: new DateTime(2026, 3, 31));

        Assert.Equal(1_500m, historical);
        Assert.Equal(3_000m, periodEnd);
    }

    [Fact]
    public void ReportCalculator_KeepsLastKnownWeightWhenLocationBalanceIsZero()
    {
        var movements = new List<BatchMovement>
        {
            Movement(2, 20, new DateTime(2026, 1, 1), BatchMovementType.Stocking, 10, 250m),
            Movement(2, 20, new DateTime(2026, 2, 1), BatchMovementType.Shipment, -10, 250m),
        };

        var reported = MortalityReportBiomassCalculator.CalculateReportedBiomassGram(
            MortalityBiomassCalculationMode.PeriodEndLatestWeight,
            deadCount: 4,
            historicalActualBiomassGram: 0m,
            movements,
            fishBatchId: 2,
            projectCageId: 20,
            periodEnd: new DateTime(2026, 2, 28));

        Assert.Equal(500m, reported);
    }

    private static BatchMovement Movement(
        long fishBatchId,
        long projectCageId,
        DateTime date,
        BatchMovementType type,
        int signedCount,
        decimal averageGram)
    {
        return new BatchMovement
        {
            FishBatchId = fishBatchId,
            ProjectCageId = projectCageId,
            MovementDate = date,
            MovementType = type,
            SignedCount = signedCount,
            SignedBiomassGram = signedCount * averageGram,
            FromAverageGram = averageGram,
            ToAverageGram = averageGram,
            IsDeleted = false,
        };
    }
}
