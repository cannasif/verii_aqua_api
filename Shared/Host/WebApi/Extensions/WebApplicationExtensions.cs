using aqua_api.Shared.Infrastructure.Persistence.Data;
using aqua_api.Shared.Host.WebApi.Hubs;
using Hangfire;
using Hangfire.Dashboard;
using aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace aqua_api.Shared.Host.WebApi.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseAquaApiWebApi(this WebApplication app, string[] configuredCorsOrigins)
    {
        var isTesting = app.Environment.IsEnvironment("Testing");

        if (!isTesting)
        {
            GlobalJobFilters.Filters.Add(
                new HangfireJobStateFilter(
                    app.Services.GetRequiredService<ILogger<HangfireJobStateFilter>>(),
                    app.Services.GetRequiredService<IBackgroundJobClient>(),
                    app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<HangfireMonitoringOptions>>(),
                    app.Services.GetRequiredService<IServiceScopeFactory>()));
        }

        var allowedCorsOrigins = CorsOriginMatcher.NormalizeAllowedOrigins(configuredCorsOrigins);

        app.Use(async (ctx, next) =>
        {
            var origin = ctx.Request.Headers["Origin"].ToString();
            if (CorsOriginMatcher.IsAllowed(origin, allowedCorsOrigins, ctx.Request.Host))
            {
                ctx.Response.OnStarting(() =>
                {
                    ApplyCorsHeaders(ctx, origin);
                    return Task.CompletedTask;
                });

                if (HttpMethods.IsOptions(ctx.Request.Method))
                {
                    ApplyCorsHeaders(ctx, origin);
                    ctx.Response.StatusCode = StatusCodes.Status204NoContent;
                    return;
                }
            }

            await next();
        });

        app.UseExceptionHandler(errApp =>
        {
            errApp.Run(async ctx =>
            {
                var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
                var logger = ctx.RequestServices.GetService<ILogger<Program>>();
                var localizationService = ctx.RequestServices.GetService<ILocalizationService>();

                if (ex != null)
                {
                    logger?.LogError(ex, "Unhandled exception: {Path}", ctx.Request.Path);
                }

                var dbUpdateException = FindDbUpdateException(ex);
                if (dbUpdateException != null &&
                    DbUpdateExceptionHelper.TryGetUniqueViolation(dbUpdateException, out _))
                {
                    var isCountryPath = ctx.Request.Path.StartsWithSegments("/api/Country", StringComparison.OrdinalIgnoreCase);
                    var localizedMessage = isCountryPath
                        ? localizationService?.GetLocalizedString("CountryService.CountryNameAlreadyExists")
                            ?? LocalizationBootstrap.GetString("CountryService.CountryNameAlreadyExists")
                        : localizationService?.GetLocalizedString("General.RecordAlreadyExists")
                            ?? LocalizationBootstrap.GetString("General.RecordAlreadyExists");

                    var response = ApiResponse<object>.ErrorResult(
                        localizedMessage,
                        null,
                        StatusCodes.Status409Conflict);
                    response.Errors = new List<string> { localizedMessage };
                    response.Timestamp = DateTime.UtcNow;
                    response.ExceptionMessage = null!;

                    if (isCountryPath)
                    {
                        response.ClassName = "ApiResponse<CountryGetDto>";
                    }

                    ctx.Response.StatusCode = StatusCodes.Status409Conflict;
                    ctx.Response.ContentType = "application/json";

                    var conflictJson = System.Text.Json.JsonSerializer.Serialize(response);
                    await ctx.Response.WriteAsync(conflictJson);
                    return;
                }

                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                ctx.Response.ContentType = "application/json";

                var fallbackMessage = localizationService?.GetLocalizedString("General.ErrorOccurred")
                    ?? LocalizationBootstrap.GetString("General.ErrorOccurred");
                var message = ex?.Message ?? fallbackMessage;
                var json = System.Text.Json.JsonSerializer.Serialize(new { error = fallbackMessage, message });
                await ctx.Response.WriteAsync(json);
            });
        });

        app.Use(async (ctx, next) =>
        {
            if (HttpMethods.IsPost(ctx.Request.Method))
            {
                if (TryResolvePostVerbFallback(ctx.Request.Path, out var normalizedPath, out var normalizedMethod))
                {
                    ctx.Request.Method = normalizedMethod;
                    ctx.Request.Path = normalizedPath;
                }
                else if (ctx.Request.Headers.TryGetValue("X-HTTP-Method-Override", out var overrideMethod) ||
                         ctx.Request.Query.TryGetValue("__method", out overrideMethod))
                {
                    var method = overrideMethod.ToString().Trim().ToUpperInvariant();
                    if (method is "PUT" or "PATCH" or "DELETE")
                    {
                        ctx.Request.Method = method;
                    }
                }
            }

            await next();
        });

        app.UseRouting();
        app.UseCors("DevCors");

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Aqua Web API v1");
                options.RoutePrefix = "swagger";
            });
        }

        app.UseStaticFiles();

        var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
        if (Directory.Exists(uploadsPath))
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadsPath),
                RequestPath = "/uploads"
            });
        }

        app.UseRequestLocalization();
        app.UseMiddleware<BranchCodeMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHub<AuthHub>("/authHub");
        app.MapHub<NotificationHub>("/notificationHub");
        app.MapControllers();

        if (!isTesting)
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });

            var enableStockSyncInDevelopment = app.Configuration.GetValue<bool>("Hangfire:StockSync:EnableInDevelopment");
            var shouldRunRecurringStockSync = !app.Environment.IsDevelopment() || enableStockSyncInDevelopment;

            if (shouldRunRecurringStockSync)
            {
                RecurringJob.AddOrUpdate<IStockSyncJob>(
                    "erp-stock-sync-job",
                    job => job.ExecuteAsync(),
                    "5,35 * * * *");
                RecurringJob.AddOrUpdate<IWarehouseSyncJob>(
                    "erp-warehouse-sync-job",
                    job => job.ExecuteAsync(),
                    "15,45 * * * *");
                RecurringJob.AddOrUpdate<IErpReceiptShipmentMovementSyncJob>(
                    "erp-receipt-shipment-movement-sync-job",
                    job => job.ExecuteAsync(),
                    "25,55 * * * *");
                RecurringJob.AddOrUpdate<IDailyErpWarehouseIssueJob>(
                    "daily-erp-warehouse-issue-job",
                    job => job.ExecuteAsync(),
                    "50 23 * * *",
                    new RecurringJobOptions
                    {
                        TimeZone = ResolveTurkeyTimeZone()
                    });
            }
            else
            {
                RecurringJob.RemoveIfExists("erp-stock-sync-job");
                RecurringJob.RemoveIfExists("erp-warehouse-sync-job");
                RecurringJob.RemoveIfExists("erp-receipt-shipment-movement-sync-job");
                RecurringJob.RemoveIfExists("daily-erp-warehouse-issue-job");
                app.Logger.LogInformation("Skipping recurring ERP sync jobs in Development environment. Set Hangfire:StockSync:EnableInDevelopment=true to enable.");
            }
        }

        return app;
    }

    private static void ApplyCorsHeaders(HttpContext ctx, string origin)
    {
        if (ctx.Response.HasStarted)
        {
            return;
        }

        ctx.Response.Headers["Access-Control-Allow-Origin"] = origin;
        ctx.Response.Headers["Access-Control-Allow-Credentials"] = "true";
        ctx.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, PATCH, DELETE, OPTIONS";
        ctx.Response.Headers["Access-Control-Allow-Headers"] =
            "Content-Type, Authorization, X-Branch-Code, Branch-Code, X-Language, x-language, x-branch-code, X-Requested-With, x-requested-with, X-SignalR-User-Agent, x-signalr-user-agent, X-HTTP-Method-Override, x-http-method-override";
        ctx.Response.Headers["Access-Control-Max-Age"] = "86400";

        var vary = ctx.Response.Headers.Vary.ToString();
        if (!vary.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(value => string.Equals(value, "Origin", StringComparison.OrdinalIgnoreCase)))
        {
            ctx.Response.Headers.Vary = string.IsNullOrWhiteSpace(vary) ? "Origin" : $"{vary}, Origin";
        }
    }

    private static TimeZoneInfo ResolveTurkeyTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
    }

    private static bool TryResolvePostVerbFallback(PathString requestPath, out PathString normalizedPath, out string normalizedMethod)
    {
        var path = requestPath.Value ?? string.Empty;
        normalizedPath = requestPath;
        normalizedMethod = HttpMethods.Post;

        if (path.EndsWith("/update", StringComparison.OrdinalIgnoreCase))
        {
            normalizedPath = new PathString(path[..^"/update".Length]);
            normalizedMethod = HttpMethods.Put;
            return HasNumericTailSegment(normalizedPath);
        }

        if (path.EndsWith("/delete", StringComparison.OrdinalIgnoreCase))
        {
            normalizedPath = new PathString(path[..^"/delete".Length]);
            normalizedMethod = HttpMethods.Delete;
            return HasNumericTailSegment(normalizedPath);
        }

        return false;
    }

    private static bool HasNumericTailSegment(PathString path)
    {
        var value = path.Value?.TrimEnd('/') ?? string.Empty;
        var lastSlashIndex = value.LastIndexOf('/');
        if (lastSlashIndex < 0 || lastSlashIndex == value.Length - 1)
        {
            return false;
        }

        var tail = value[(lastSlashIndex + 1)..];
        return long.TryParse(tail, out _);
    }

    private static DbUpdateException? FindDbUpdateException(Exception? exception)
    {
        var current = exception;
        while (current != null)
        {
            if (current is DbUpdateException dbUpdateException)
            {
                return dbUpdateException;
            }

            current = current.InnerException;
        }

        return null;
    }
}
