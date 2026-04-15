namespace aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces
{
    public interface IWarehouseSyncJob
    {
        Task ExecuteAsync();
    }
}
