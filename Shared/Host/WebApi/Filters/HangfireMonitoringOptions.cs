namespace aqua_api.Shared.Host.WebApi.Filters
{
    public class HangfireMonitoringOptions
    {
        public const string SectionName = "HangfireMonitoring";

        public bool EnableFailureSqlLog { get; set; } = true;
        public int FinalRetryCountThreshold { get; set; } = 3;
        public List<string> CriticalJobs { get; set; } = new();
        public List<string> AlertEmails { get; set; } = new();
    }
}
