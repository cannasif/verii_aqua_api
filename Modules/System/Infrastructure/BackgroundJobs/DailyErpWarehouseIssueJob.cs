using Hangfire;
using aqua_api.Modules.Feedings.Application.Services;
using aqua_api.Modules.Mortalities.Application.Services;
using aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces;

namespace aqua_api.Modules.System.Infrastructure.BackgroundJobs
{
    public class DailyErpWarehouseIssueJob : IDailyErpWarehouseIssueJob
    {
        private const long SystemUserId = 1;
        private readonly IFeedingService _feedingService;
        private readonly IMortalityService _mortalityService;
        private readonly ILogger<DailyErpWarehouseIssueJob> _logger;

        public DailyErpWarehouseIssueJob(
            IFeedingService feedingService,
            IMortalityService mortalityService,
            ILogger<DailyErpWarehouseIssueJob> logger)
        {
            _feedingService = feedingService;
            _mortalityService = mortalityService;
            _logger = logger;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 3600)]
        public async Task ExecuteAsync()
        {
            var operationDate = DateTimeProvider.UtcNow.Date;
            _logger.LogInformation("Daily ERP warehouse issue job started. OperationDate: {OperationDate}", operationDate);

            var feedingCount = await _feedingService.ProcessPendingErpIntegrationsAsync(operationDate, SystemUserId);
            var mortalityCount = await _mortalityService.ProcessPendingErpIntegrationsAsync(operationDate, SystemUserId);

            _logger.LogInformation(
                "Daily ERP warehouse issue job completed. OperationDate: {OperationDate}, Feedings: {FeedingCount}, Mortalities: {MortalityCount}",
                operationDate,
                feedingCount,
                mortalityCount);
        }
    }
}
