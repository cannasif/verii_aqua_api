using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces;

namespace aqua_api.Modules.System.Api
{
    [ApiController]
    [Route("api/hangfire")]
    [Authorize]
    public class HangfireController : ControllerBase
    {
        private const string StockSyncRecurringJobId = "erp-stock-sync-job";
        private const string WarehouseSyncRecurringJobId = "erp-warehouse-sync-job";

        private readonly AquaDbContext _db;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILocalizationService _localizationService;

        public HangfireController(AquaDbContext db, IBackgroundJobClient backgroundJobClient, ILocalizationService localizationService)
        {
            _db = db;
            _backgroundJobClient = backgroundJobClient;
            _localizationService = localizationService;
        }

        [HttpGet("recurring-jobs")]
        public IActionResult GetRecurringJobs()
        {
            using var connection = JobStorage.Current.GetConnection();
            var jobs = connection.GetRecurringJobs()
                .OrderBy(x => x.Id)
                .Select(x => new RecurringJobListItem
                {
                    Id = x.Id,
                    JobName = x.Job?.Type?.Name ?? x.Id,
                    Method = x.Job?.Method?.Name,
                    Cron = x.Cron,
                    Queue = x.Queue,
                    NextExecution = x.NextExecution?.ToString("o"),
                    LastExecution = x.LastExecution?.ToString("o"),
                    LastJobId = x.LastJobId,
                    Error = x.Error,
                })
                .ToList();

            EnsureManualSyncJobs(jobs);

            return Ok(new
            {
                Items = jobs,
                Total = jobs.Count,
                Timestamp = DateTime.UtcNow,
            });
        }

        [HttpPost("recurring-jobs/{jobId}/trigger")]
        public IActionResult TriggerRecurringJob([FromRoute] string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                return BadRequest(new { Message = "Job id is required." });
            }

            using var connection = JobStorage.Current.GetConnection();
            var exists = connection.GetRecurringJobs()
                .Any(x => string.Equals(x.Id, jobId, StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                RecurringJob.TriggerJob(jobId);

                return Ok(new
                {
                    JobId = jobId,
                    TriggeredAt = DateTime.UtcNow,
                    Message = "Recurring job triggered successfully.",
                });
            }

            var enqueuedJobId = EnqueueKnownSyncJob(jobId);
            if (enqueuedJobId == null)
            {
                return NotFound(new { Message = "Recurring job not found." });
            }

            return Ok(new
            {
                JobId = enqueuedJobId,
                RecurringJobId = jobId,
                TriggeredAt = DateTime.UtcNow,
                Message = "Sync job enqueued successfully.",
            });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var executions = _db.JobExecutionLogs
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            var failedFromExecutions = await executions.CountAsync(x => x.Status == "Failed");
            var failed = failedFromExecutions > 0
                ? failedFromExecutions
                : await _db.JobFailureLogs.AsNoTracking().CountAsync(x => !x.IsDeleted);
            var succeeded = await executions.CountAsync(x => x.Status == "Succeeded");
            var queues = await executions
                .Where(x => !string.IsNullOrWhiteSpace(x.Queue))
                .Select(x => x.Queue!)
                .Distinct()
                .CountAsync();

            return Ok(new
            {
                Enqueued = 0,
                Processing = 0,
                Scheduled = 0,
                Succeeded = succeeded,
                Failed = failed,
                Deleted = 0,
                Servers = 0,
                Queues = queues,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("failed")]
        public Task<IActionResult> GetFailed(
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] int from = 0,
            [FromQuery] int count = 20)
        {
            var (resolvedFrom, resolvedCount) = ResolvePaging(pageNumber, pageSize, from, count, 20);
            return GetFailuresFromDb(resolvedFrom, resolvedCount);
        }

        [HttpGet("failures-from-db")]
        public async Task<IActionResult> GetFailuresFromDb(
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] int from = 0,
            [FromQuery] int count = 50)
        {
            var (resolvedFrom, resolvedCount) = ResolvePaging(pageNumber, pageSize, from, count, 50);

            var items = await _db.JobFailureLogs
                .AsNoTracking()
                .OrderByDescending(x => x.FailedAt)
                .Skip(resolvedFrom)
                .Take(resolvedCount)
                .Select(x => new
                {
                    x.JobId,
                    x.JobName,
                    FailedAt = x.FailedAt.ToString("o"),
                    State = "Failed",
                    Reason = x.ExceptionMessage ?? x.Reason,
                    x.ExceptionType,
                    x.RetryCount,
                    x.Queue
                })
                .ToListAsync();

            var total = await _db.JobFailureLogs.CountAsync();

            return Ok(new
            {
                Items = items,
                Total = total,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("successes-from-db")]
        public async Task<IActionResult> GetSuccessesFromDb(
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] int from = 0,
            [FromQuery] int count = 50)
        {
            var (resolvedFrom, resolvedCount) = ResolvePaging(pageNumber, pageSize, from, count, 50);

            var successQuery = _db.JobExecutionLogs
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.Status == "Succeeded");

            var items = await successQuery
                .OrderByDescending(x => x.FinishedAt)
                .Skip(resolvedFrom)
                .Take(resolvedCount)
                .Select(x => new
                {
                    x.JobId,
                    x.RecurringJobId,
                    x.JobName,
                    FinishedAt = x.FinishedAt.ToString("o"),
                    x.DurationMs,
                    x.Queue,
                    x.RetryCount
                })
                .ToListAsync();

            var total = await successQuery.CountAsync();

            return Ok(new
            {
                Items = items,
                Total = total,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("dead-letter")]
        public async Task<IActionResult> GetDeadLetter(
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] int from = 0,
            [FromQuery] int count = 20)
        {
            var (resolvedFrom, resolvedCount) = ResolvePaging(pageNumber, pageSize, from, count, 20);

            var deadLetterQuery = _db.JobFailureLogs
                .AsNoTracking()
                .Where(x => x.Queue == "dead-letter");

            var items = await deadLetterQuery
                .OrderByDescending(x => x.FailedAt)
                .Skip(resolvedFrom)
                .Take(resolvedCount)
                .Select(x => new
                {
                    x.JobId,
                    x.JobName,
                    EnqueuedAt = x.FailedAt.ToString("o"),
                    State = "Enqueued",
                    Reason = x.ExceptionMessage ?? x.Reason
                })
                .ToListAsync();

            var total = await deadLetterQuery.CountAsync();

            return Ok(new
            {
                Queue = "dead-letter",
                Enqueued = total,
                Items = items,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("stock-sync/run-now")]
        public IActionResult RunStockSyncNow()
        {
            var jobId = _backgroundJobClient.Enqueue<IStockSyncJob>(job => job.ExecuteAsync());
            return Ok(new
            {
                Message = _localizationService.GetLocalizedString("HangfireController.StockSyncJobEnqueued"),
                JobId = jobId,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("warehouse-sync/run-now")]
        public IActionResult RunWarehouseSyncNow()
        {
            var jobId = _backgroundJobClient.Enqueue<IWarehouseSyncJob>(job => job.ExecuteAsync());
            return Ok(new
            {
                Message = _localizationService.GetLocalizedString("HangfireController.WarehouseSyncJobEnqueued"),
                JobId = jobId,
                Timestamp = DateTime.UtcNow
            });
        }

        private static (int From, int Count) ResolvePaging(
            int? pageNumber,
            int? pageSize,
            int from,
            int count,
            int fallbackCount)
        {
            if (pageNumber is null && pageSize is not null && from == 0)
            {
                return (0, NormalizePageSize(pageSize.Value, fallbackCount));
            }

            if (pageNumber.HasValue)
            {
                var safePageSize = NormalizePageSize(pageSize ?? fallbackCount, fallbackCount);
                var safePageNumber = Math.Max(1, pageNumber.Value);
                var computedFrom = (safePageNumber - 1) * safePageSize;

                return (computedFrom, safePageSize);
            }

            var safeFrom = Math.Max(0, from);
            return (safeFrom, NormalizePageSize(count, fallbackCount));
        }

        private static int NormalizePageSize(int requestedSize, int fallbackSize)
        {
            var parsed = requestedSize <= 0 ? fallbackSize : requestedSize;
            return Math.Min(200, Math.Max(1, parsed));
        }

        private static void EnsureManualSyncJobs(List<RecurringJobListItem> jobs)
        {
            AddManualJobIfMissing(
                jobs,
                StockSyncRecurringJobId,
                nameof(StockSyncJob),
                nameof(IStockSyncJob.ExecuteAsync));

            AddManualJobIfMissing(
                jobs,
                WarehouseSyncRecurringJobId,
                nameof(WarehouseSyncJob),
                nameof(IWarehouseSyncJob.ExecuteAsync));
        }

        private static void AddManualJobIfMissing(
            List<RecurringJobListItem> jobs,
            string id,
            string jobName,
            string method)
        {
            if (jobs.Any(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            jobs.Add(new RecurringJobListItem
            {
                Id = id,
                JobName = jobName,
                Method = method,
                Cron = "-",
                Queue = "default",
                Error = null,
            });
        }

        private string? EnqueueKnownSyncJob(string jobId)
        {
            if (string.Equals(jobId, StockSyncRecurringJobId, StringComparison.OrdinalIgnoreCase))
            {
                return _backgroundJobClient.Enqueue<IStockSyncJob>(job => job.ExecuteAsync());
            }

            if (string.Equals(jobId, WarehouseSyncRecurringJobId, StringComparison.OrdinalIgnoreCase))
            {
                return _backgroundJobClient.Enqueue<IWarehouseSyncJob>(job => job.ExecuteAsync());
            }

            return null;
        }

        private sealed class RecurringJobListItem
        {
            public string Id { get; set; } = string.Empty;
            public string JobName { get; set; } = string.Empty;
            public string? Method { get; set; }
            public string? Cron { get; set; }
            public string? Queue { get; set; }
            public string? NextExecution { get; set; }
            public string? LastExecution { get; set; }
            public string? LastJobId { get; set; }
            public string? Error { get; set; }
        }
    }
}
