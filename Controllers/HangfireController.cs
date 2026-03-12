using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aqua_api.Data;
using Infrastructure.BackgroundJobs.Interfaces;

namespace aqua_api.Controllers
{
    [ApiController]
    [Route("api/hangfire")]
    [Authorize]
    public class HangfireController : ControllerBase
    {
        private readonly AquaDbContext _db;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HangfireController(AquaDbContext db, IBackgroundJobClient backgroundJobClient)
        {
            _db = db;
            _backgroundJobClient = backgroundJobClient;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var failed = await _db.JobFailureLogs
                .AsNoTracking()
                .CountAsync();

            return Ok(new
            {
                Enqueued = 0,
                Processing = 0,
                Scheduled = 0,
                Succeeded = 0,
                Failed = failed,
                Deleted = 0,
                Servers = 0,
                Queues = 1,
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
                Message = "Stock sync job enqueued.",
                JobId = jobId,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
