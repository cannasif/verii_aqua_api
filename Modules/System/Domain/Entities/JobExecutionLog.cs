namespace aqua_api.Modules.System.Domain.Entities
{
    public class JobExecutionLog : BaseEntity
    {
        public string JobId { get; set; } = string.Empty;
        public string? RecurringJobId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Queue { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }
        public int DurationMs { get; set; }
        public string? Reason { get; set; }
        public string? ExceptionType { get; set; }
        public string? ExceptionMessage { get; set; }
        public int RetryCount { get; set; }
    }
}
