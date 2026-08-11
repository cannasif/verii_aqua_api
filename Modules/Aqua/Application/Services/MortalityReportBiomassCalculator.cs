namespace aqua_api.Modules.Aqua.Application.Services
{
    public static class MortalityReportBiomassCalculator
    {
        public static decimal CalculateReportedBiomassGram(
            MortalityBiomassCalculationMode mode,
            int deadCount,
            decimal historicalActualBiomassGram,
            IEnumerable<BatchMovement> movements,
            long fishBatchId,
            long projectCageId,
            DateTime periodEnd)
        {
            if (mode == MortalityBiomassCalculationMode.HistoricalEventWeight)
            {
                return MortalityBiomassMath.CalculateReportedBiomassGram(historicalActualBiomassGram);
            }

            var latestAverageGram = ResolveLatestAverageGram(
                movements,
                fishBatchId,
                projectCageId,
                periodEnd);

            return latestAverageGram > 0m
                ? MortalityBiomassMath.CalculateReportedBiomassGram(deadCount, latestAverageGram)
                : MortalityBiomassMath.CalculateReportedBiomassGram(historicalActualBiomassGram);
        }

        public static decimal CalculateReportedBiomassKg(
            MortalityBiomassCalculationMode mode,
            int deadCount,
            decimal historicalActualBiomassGram,
            IEnumerable<BatchMovement> movements,
            long fishBatchId,
            long projectCageId,
            DateTime periodEnd)
        {
            return Math.Round(
                CalculateReportedBiomassGram(
                    mode,
                    deadCount,
                    historicalActualBiomassGram,
                    movements,
                    fishBatchId,
                    projectCageId,
                    periodEnd) / 1000m,
                3,
                MidpointRounding.AwayFromZero);
        }

        public static decimal ResolveLatestAverageGram(
            IEnumerable<BatchMovement> movements,
            long fishBatchId,
            long projectCageId,
            DateTime periodEnd)
        {
            var locationMovements = movements
                .Where(x =>
                    !x.IsDeleted &&
                    x.FishBatchId == fishBatchId &&
                    x.ProjectCageId == projectCageId &&
                    !x.WarehouseId.HasValue &&
                    x.MovementDate.Date <= periodEnd.Date)
                .ToList();

            var snapshot = BatchReportMassCalculator.CalculateSnapshot(locationMovements);
            if (snapshot.AverageGram > 0m)
            {
                return snapshot.AverageGram;
            }

            // Tamamen sevk edilen veya fireyle kapanan bir konumda bakiye sifir olsa da
            // rapor donemi icindeki son bilinen gramaj gecerliligini korur.
            return locationMovements
                .OrderByDescending(x => x.MovementDate)
                .ThenByDescending(x => x.CreatedDate)
                .ThenByDescending(x => x.Id)
                .Select(BatchReportMassCalculator.ResolveMovementAverageGram)
                .FirstOrDefault(x => x > 0m);
        }
    }
}
