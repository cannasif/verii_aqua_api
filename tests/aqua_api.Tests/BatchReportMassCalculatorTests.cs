using aqua_api.Modules.Aqua.Application.Services;
using aqua_api.Modules.Aqua.Domain.Enums;
using Xunit;

namespace aqua_api.Tests;

public sealed class BatchReportMassCalculatorTests
{
    [Fact]
    public void Snapshot_UsesLatestGrowthGramAndCountInsteadOfStoredBiomass()
    {
        var movements = new[]
        {
            Movement(1, new DateTime(2026, 1, 1), BatchMovementType.Stocking, 1_000, 1m, 100m, 100m),
            Movement(2, new DateTime(2026, 2, 1), BatchMovementType.FishGrowth, 0, 1m, 100m, 200m),
            Movement(3, new DateTime(2026, 3, 1), BatchMovementType.FishGrowth, 0, 1m, 200m, 300m),
            Movement(4, new DateTime(2026, 3, 20), BatchMovementType.Shipment, -100, -1m, 300m, null),
            Movement(5, new DateTime(2026, 3, 21), BatchMovementType.Mortality, -10, -1m, 300m, null),
        };

        var snapshot = BatchReportMassCalculator.CalculateSnapshot(movements);
        var deltas = BatchReportMassCalculator.CalculateMovementBiomassDeltas(movements);

        Assert.Equal(890, snapshot.LiveCount);
        Assert.Equal(300m, snapshot.AverageGram);
        Assert.Equal(267_000m, snapshot.BiomassGram);
        Assert.Equal(100_000m, deltas[1]);
        Assert.Equal(100_000m, deltas[2]);
        Assert.Equal(100_000m, deltas[3]);
        Assert.Equal(-30_000m, deltas[4]);
        Assert.Equal(-3_000m, deltas[5]);
        Assert.Equal(-30_000m, BatchReportMassCalculator.CalculateSignedBiomassGram(movements[3]));
        Assert.Equal(-3_000m, BatchReportMassCalculator.CalculateSignedBiomassGram(movements[4]));
    }

    [Fact]
    public void Snapshot_CalculatesEachBatchSeparatelyBeforeSummingBiomass()
    {
        var movements = new[]
        {
            Movement(1, new DateTime(2026, 1, 1), BatchMovementType.Stocking, 100, 999m, 100m, 100m, fishBatchId: 10),
            Movement(2, new DateTime(2026, 1, 1), BatchMovementType.Stocking, 200, 999m, 250m, 250m, fishBatchId: 20),
        };

        var snapshot = BatchReportMassCalculator.CalculateSnapshot(movements);

        Assert.Equal(300, snapshot.LiveCount);
        Assert.Equal(60_000m, snapshot.BiomassGram);
        Assert.Equal(200m, snapshot.AverageGram);
    }

    [Fact]
    public void Snapshot_UsesExactBiomassDelta_WhenErpCorrectsCountAndGramTogether()
    {
        var opening = Movement(
            1,
            new DateTime(2026, 1, 1),
            BatchMovementType.Stocking,
            100,
            1_400m,
            null,
            14m);
        var legacyCorrection = Movement(
            2,
            new DateTime(2026, 1, 1),
            BatchMovementType.Stocking,
            20,
            520m,
            null,
            16m);
        legacyCorrection.Note = "ERP fish receipt delta | projectId=1";

        var movements = new[] { opening, legacyCorrection };
        var snapshot = BatchReportMassCalculator.CalculateSnapshot(movements);
        var deltas = BatchReportMassCalculator.CalculateMovementBiomassDeltas(movements);

        Assert.Equal(120, snapshot.LiveCount);
        Assert.Equal(16m, snapshot.AverageGram);
        Assert.Equal(1_920m, snapshot.BiomassGram);
        Assert.Equal(1_400m, deltas[1]);
        Assert.Equal(520m, deltas[2]);
        Assert.Equal(520m, BatchReportMassCalculator.CalculateSignedBiomassGram(legacyCorrection));
    }

    private static BatchMovement Movement(
        long id,
        DateTime date,
        BatchMovementType type,
        int count,
        decimal storedBiomassGram,
        decimal? fromAverageGram,
        decimal? toAverageGram,
        long fishBatchId = 1)
    {
        return new BatchMovement
        {
            Id = id,
            FishBatchId = fishBatchId,
            ProjectCageId = 1,
            MovementDate = date,
            MovementType = type,
            SignedCount = count,
            SignedBiomassGram = storedBiomassGram,
            FromAverageGram = fromAverageGram,
            ToAverageGram = toAverageGram,
            CreatedDate = date,
        };
    }
}
