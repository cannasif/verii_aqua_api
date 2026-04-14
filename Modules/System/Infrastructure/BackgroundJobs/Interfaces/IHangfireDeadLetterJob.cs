namespace aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces
{
    public interface IHangfireDeadLetterJob
    {
        Task ProcessAsync(HangfireDeadLetterPayload payload);
    }
}
