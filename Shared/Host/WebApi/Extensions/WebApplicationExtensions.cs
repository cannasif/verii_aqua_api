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
        GlobalJobFilters.Filters.Add(
            new HangfireJobStateFilter(
                app.Services.GetRequiredService<ILogger<HangfireJobStateFilter>>(),
                app.Services.GetRequiredService<IBackgroundJobClient>(),
                app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<HangfireMonitoringOptions>>(),
                app.Services.GetRequiredService<IServiceScopeFactory>()));

        var allowedCorsOrigins = new HashSet<string>(configuredCorsOrigins, StringComparer.OrdinalIgnoreCase);

        app.Use(async (ctx, next) =>
        {
            var origin = ctx.Request.Headers["Origin"].ToString();
            if (!string.IsNullOrEmpty(origin) && allowedCorsOrigins.Contains(origin))
            {
                ctx.Response.Headers.Append("Access-Control-Allow-Origin", origin);
                ctx.Response.Headers.Append("Access-Control-Allow-Credentials", "true");

                if (HttpMethods.IsOptions(ctx.Request.Method))
                {
                    ctx.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, PATCH, DELETE, OPTIONS");
                    ctx.Response.Headers.Append("Access-Control-Allow-Headers",
                        "Content-Type, Authorization, X-Branch-Code, Branch-Code, X-Language, x-language, x-branch-code");
                    ctx.Response.Headers.Append("Access-Control-Max-Age", "86400");
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

                    var conflictOrigin = ctx.Request.Headers["Origin"].ToString();
                    if (!string.IsNullOrEmpty(conflictOrigin) && allowedCorsOrigins.Contains(conflictOrigin))
                    {
                        if (!ctx.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
                        {
                            ctx.Response.Headers.Append("Access-Control-Allow-Origin", conflictOrigin);
                            ctx.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
                        }
                    }

                    var conflictJson = System.Text.Json.JsonSerializer.Serialize(response);
                    await ctx.Response.WriteAsync(conflictJson);
                    return;
                }

                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                ctx.Response.ContentType = "application/json";

                var origin = ctx.Request.Headers["Origin"].ToString();
                if (!string.IsNullOrEmpty(origin) && allowedCorsOrigins.Contains(origin))
                {
                    if (!ctx.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
                    {
                        ctx.Response.Headers.Append("Access-Control-Allow-Origin", origin);
                        ctx.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
                    }
                }

                var fallbackMessage = localizationService?.GetLocalizedString("General.ErrorOccurred")
                    ?? LocalizationBootstrap.GetString("General.ErrorOccurred");
                var message = ex?.Message ?? fallbackMessage;
                var json = System.Text.Json.JsonSerializer.Serialize(new { error = fallbackMessage, message });
                await ctx.Response.WriteAsync(json);
            });
        });

        app.UseRouting();
        app.UseCors("DevCors");

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM Web API v1");
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
                Cron.MinuteInterval(30));
            RecurringJob.AddOrUpdate<IWarehouseSyncJob>(
                "erp-warehouse-sync-job",
                job => job.ExecuteAsync(),
                Cron.MinuteInterval(30));
        }
        else
        {
            RecurringJob.RemoveIfExists("erp-stock-sync-job");
            RecurringJob.RemoveIfExists("erp-warehouse-sync-job");
            app.Logger.LogInformation("Skipping recurring ERP sync jobs in Development environment. Set Hangfire:StockSync:EnableInDevelopment=true to enable.");
        }

        return app;
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
