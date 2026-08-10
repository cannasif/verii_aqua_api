namespace aqua_api.Shared.Common.Helpers
{
    public static class MortalityBiomassMath
    {
        public const decimal ReportedWeightRatio = 0.5m;

        public static decimal CalculateReportedBiomassGram(int deadCount, decimal averageGram)
        {
            var actualBiomassGram = BatchMath.CalculateBiomassGram(
                Math.Max(0, deadCount),
                Math.Max(0m, averageGram));

            return CalculateReportedBiomassGram(actualBiomassGram);
        }

        public static decimal CalculateReportedBiomassGram(decimal actualBiomassGram)
        {
            return CalculateReportedBiomassKgFromActualGram(actualBiomassGram) * 1000m;
        }

        public static decimal CalculateReportedBiomassKg(int deadCount, decimal averageGram)
        {
            return CalculateReportedBiomassKgFromActualGram(
                BatchMath.CalculateBiomassGram(Math.Max(0, deadCount), Math.Max(0m, averageGram)));
        }

        public static decimal CalculateReportedBiomassKgFromActualGram(decimal actualBiomassGram)
        {
            var actualBiomassKg = Math.Round(
                Math.Max(0m, actualBiomassGram) / 1000m,
                3,
                MidpointRounding.AwayFromZero);

            return Math.Round(
                actualBiomassKg * ReportedWeightRatio,
                3,
                MidpointRounding.AwayFromZero);
        }
    }
}
