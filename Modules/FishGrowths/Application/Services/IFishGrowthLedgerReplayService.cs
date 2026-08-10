namespace aqua_api.Modules.FishGrowths.Application.Services;

public sealed record FishGrowthLedgerState(
    int FishCount,
    decimal BiomassGram,
    decimal AverageGram);

public interface IFishGrowthLedgerReplayService
{
    Task PrepareAsync(
        long projectId,
        long fishBatchId,
        long projectCageId,
        long? userId);

    Task<FishGrowthLedgerState> GetStateBeforeAsync(
        long fishBatchId,
        long projectCageId,
        DateTime effectiveDate);

    Task ReplayAsync(
        long projectId,
        long fishBatchId,
        long projectCageId,
        DateTime fromDate,
        long? userId);
}
