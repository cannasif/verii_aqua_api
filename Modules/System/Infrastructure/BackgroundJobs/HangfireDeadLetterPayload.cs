namespace aqua_api.Modules.System.Infrastructure.BackgroundJobs
{
    public class HangfireDeadLetterPayload
    {
        public string JobId { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public string? Queue { get; set; }
        public int RetryCount { get; set; }
        public string? Reason { get; set; }
        public string? ExceptionType { get; set; }
        public string? ExceptionMessage { get; set; }
        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    }
}
