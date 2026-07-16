namespace aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces
{
    public interface IErpReceiptShipmentMovementSyncJob
    {
        Task ExecuteAsync();
        Task ProcessMovementInCurrentTransactionAsync(MalKabulVeSevkiyatDto movement);
    }
}
