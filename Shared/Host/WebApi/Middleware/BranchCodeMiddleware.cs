using Microsoft.AspNetCore.Http;

namespace aqua_api.Shared.Host.WebApi.Middleware
{
    /// <summary>
    /// Middleware to extract BranchCode from request header and store it in HttpContext.Items
    /// Header name: "X-Branch-Code" or "Branch-Code"
    /// </summary>
    public class BranchCodeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BranchCodeMiddleware> _logger;

        public BranchCodeMiddleware(RequestDelegate next, ILogger<BranchCodeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (HttpMethods.IsOptions(context.Request.Method))
            {
                await _next(context);
                return;
            }

            // Try to get BranchCode from header
            // Support both "X-Branch-Code" and "Branch-Code" header names
            var branchCode = context.Request.Headers["X-Branch-Code"].FirstOrDefault() 
                          ?? context.Request.Headers["Branch-Code"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(branchCode))
            {
                // Store in HttpContext.Items for later use
                context.Items["BranchCode"] = branchCode;
                _logger.LogDebug("BranchCode header present in request.");
            }
            else
            {
                // Not all requests require a branch context (e.g. login, health checks, pre-branch selection).
                _logger.LogDebug("BranchCode header not found in request.");
            }

            // Continue to next middleware
            await _next(context);
        }
    }
}
