namespace aqua_api.Modules.Aqua.Application.Services
{
    public readonly record struct BatchReportMassSnapshot(
        int LiveCount,
        decimal AverageGram,
        decimal BiomassGram);

    public static class BatchReportMassCalculator
    {
        public static BatchReportMassSnapshot CalculateSnapshot(IEnumerable<BatchMovement> movements)
        {
            var snapshots = movements
                .Where(x => !x.IsDeleted)
                .GroupBy(x => new MovementLocationKey(x.FishBatchId, x.ProjectCageId, x.WarehouseId))
                .Select(CalculateLocationSnapshot)
                .ToList();

            var liveCount = snapshots.Sum(x => (long)x.LiveCount);
            var biomassGram = snapshots.Sum(x => x.BiomassGram);
            var averageGram = liveCount > 0
                ? Round(biomassGram / liveCount)
                : 0m;

            return new BatchReportMassSnapshot(
                checked((int)Math.Max(0L, liveCount)),
                averageGram,
                Round(Math.Max(0m, biomassGram)));
        }

        public static decimal CalculateSignedBiomassGram(BatchMovement movement)
        {
            if (movement.SignedCount == 0)
            {
                return 0m;
            }

            var averageGram = ResolveMovementAverageGram(movement);
            if (averageGram > 0m)
            {
                return Round(movement.SignedCount * averageGram);
            }

            // Legacy movements may predate the operation-time average columns.
            return Round(movement.SignedBiomassGram);
        }

        public static decimal CalculateBiomassGram(int fishCount, decimal averageGram)
        {
            if (fishCount <= 0 || averageGram <= 0m)
            {
                return 0m;
            }

            return Round(fishCount * averageGram);
        }

        public static IReadOnlyDictionary<long, decimal> CalculateMovementBiomassDeltas(
            IEnumerable<BatchMovement> movements)
        {
            var result = new Dictionary<long, decimal>();
            foreach (var movementGroup in movements
                         .Where(x => !x.IsDeleted)
                         .GroupBy(x => new MovementLocationKey(x.FishBatchId, x.ProjectCageId, x.WarehouseId)))
            {
                long liveCount = 0;
                decimal biomassGram = 0m;
                foreach (var movement in OrderMovements(movementGroup))
                {
                    var previousBiomassGram = biomassGram;
                    ApplyMovement(movement, ref liveCount, ref biomassGram);
                    if (movement.Id > 0)
                    {
                        result[movement.Id] = Round(biomassGram - previousBiomassGram);
                    }
                }
            }

            return result;
        }

        public static decimal ResolveMovementAverageGram(BatchMovement movement)
        {
            decimal? preferredAverage = movement.SignedCount switch
            {
                < 0 => movement.FromAverageGram,
                > 0 => movement.ToAverageGram,
                _ => movement.ToAverageGram,
            };
            var secondaryAverage = movement.SignedCount < 0
                ? movement.ToAverageGram
                : movement.FromAverageGram;

            if (preferredAverage is > 0m)
            {
                return Round(preferredAverage.Value);
            }

            if (secondaryAverage is > 0m)
            {
                return Round(secondaryAverage.Value);
            }

            if (movement.SignedCount != 0 && movement.SignedBiomassGram != 0m)
            {
                return Round(Math.Abs(movement.SignedBiomassGram / movement.SignedCount));
            }

            return 0m;
        }

        private static BatchReportMassSnapshot CalculateLocationSnapshot(
            IGrouping<MovementLocationKey, BatchMovement> movementGroup)
        {
            long liveCount = 0;
            decimal biomassGram = 0m;

            foreach (var movement in OrderMovements(movementGroup))
            {
                ApplyMovement(movement, ref liveCount, ref biomassGram);
            }

            if (liveCount <= 0)
            {
                return new BatchReportMassSnapshot(0, 0m, 0m);
            }

            biomassGram = Round(Math.Max(0m, biomassGram));
            var averageGram = biomassGram > 0m
                ? Round(biomassGram / liveCount)
                : 0m;

            return new BatchReportMassSnapshot(
                checked((int)liveCount),
                averageGram,
                biomassGram);
        }

        private static void ApplyMovement(
            BatchMovement movement,
            ref long liveCount,
            ref decimal biomassGram)
        {
            var movementAverageGram = ResolveMovementAverageGram(movement);
            if (movement.SignedCount != 0)
            {
                var nextCount = liveCount + movement.SignedCount;
                if (nextCount < 0)
                {
                    liveCount = 0;
                    biomassGram = 0m;
                    return;
                }

                biomassGram += movementAverageGram > 0m
                    ? movement.SignedCount * movementAverageGram
                    : movement.SignedBiomassGram;
                liveCount = nextCount;
                biomassGram = liveCount == 0 ? 0m : Math.Max(0m, biomassGram);
                return;
            }

            if (movement.MovementType is BatchMovementType.FishGrowth or BatchMovementType.Weighing &&
                movement.ToAverageGram is > 0m)
            {
                biomassGram = liveCount * Round(movement.ToAverageGram.Value);
                return;
            }

            if (movement.MovementType is BatchMovementType.FishGrowth or BatchMovementType.Weighing &&
                movement.SignedBiomassGram != 0m)
            {
                // Legacy average-changing movements may only contain a biomass delta.
                biomassGram = Math.Max(0m, biomassGram + movement.SignedBiomassGram);
            }
        }

        private static IOrderedEnumerable<BatchMovement> OrderMovements(IEnumerable<BatchMovement> movements) =>
            movements
                .OrderBy(x => x.MovementDate)
                .ThenBy(x => GetOperationPriority(x.MovementType))
                .ThenBy(x => x.CreatedDate)
                .ThenBy(x => x.Id);

        private static int GetOperationPriority(BatchMovementType movementType) => movementType switch
        {
            BatchMovementType.Stocking or BatchMovementType.OpeningImport => 0,
            BatchMovementType.Adjustment => 10,
            BatchMovementType.FishGrowth => 20,
            BatchMovementType.Weighing => 30,
            BatchMovementType.Transfer or BatchMovementType.WarehouseTransfer or BatchMovementType.StockConvert => 40,
            BatchMovementType.Shipment => 50,
            BatchMovementType.Mortality => 60,
            BatchMovementType.Feeding => 70,
            _ => 40,
        };

        private static decimal Round(decimal value) =>
            Math.Round(value, 3, MidpointRounding.AwayFromZero);

        private readonly record struct MovementLocationKey(
            long FishBatchId,
            long? ProjectCageId,
            long? WarehouseId);
    }
}
