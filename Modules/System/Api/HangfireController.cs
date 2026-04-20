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
                .Select(x => new
                {
                    x.Id,
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

            if (!exists)
            {
                return NotFound(new { Message = "Recurring job not found." });
            }

            RecurringJob.TriggerJob(jobId);

            return Ok(new
            {
                JobId = jobId,
                TriggeredAt = DateTime.UtcNow,
                Message = "Recurring job triggered successfully.",
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
        public Task<IActionResult> GetFailed([FromQuery] int from = 0, [FromQuery] int count = 20)
        {
            return GetFailuresFromDb(from, count);
        }

        [HttpGet("failures-from-db")]
        public async Task<IActionResult> GetFailuresFromDb([FromQuery] int from = 0, [FromQuery] int count = 50)
        {
            if (from < 0) from = 0;
            if (count <= 0) count = 50;
            if (count > 200) count = 200;

            var items = await _db.JobFailureLogs
                .AsNoTracking()
                .OrderByDescending(x => x.FailedAt)
                .Skip(from)
                .Take(count)
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
        public async Task<IActionResult> GetSuccessesFromDb([FromQuery] int from = 0, [FromQuery] int count = 50)
        {
            if (from < 0) from = 0;
            if (count <= 0) count = 50;
            if (count > 200) count = 200;

            var successQuery = _db.JobExecutionLogs
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.Status == "Succeeded");

            var items = await successQuery
                .OrderByDescending(x => x.FinishedAt)
                .Skip(from)
                .Take(count)
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
        public async Task<IActionResult> GetDeadLetter([FromQuery] int from = 0, [FromQuery] int count = 20)
        {
            if (from < 0) from = 0;
            if (count <= 0) count = 20;
            if (count > 200) count = 200;

            var deadLetterQuery = _db.JobFailureLogs
                .AsNoTracking()
                .Where(x => x.Queue == "dead-letter");

            var items = await deadLetterQuery
                .OrderByDescending(x => x.FailedAt)
                .Skip(from)
                .Take(count)
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
    }
}
