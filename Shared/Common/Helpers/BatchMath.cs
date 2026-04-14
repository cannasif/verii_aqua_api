namespace aqua_api.Shared.Common.Helpers
{
    public static class BatchMath
    {
        public static decimal CalculateIncrementedAverageGram(decimal currentAverageGram, decimal gramIncrement)
        {
            return Math.Round(currentAverageGram + gramIncrement, 3, MidpointRounding.AwayFromZero);
        }

        public static decimal CalculateBiomassGram(int fishCount, decimal averageGram)
        {
            return Math.Round(fishCount * averageGram, 3, MidpointRounding.AwayFromZero);
        }
    }
}
