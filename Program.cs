using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using aqua_api.Data;
using aqua_api.Interfaces;
using aqua_api.Mappings;
using aqua_api.Repositories;
using aqua_api.Services;
using aqua_api.UnitOfWork;
using aqua_api.Hubs;
using aqua_api.Helpers;
using System.Security.Claims;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Hangfire;
using Hangfire.SqlServer;
using Infrastructure.BackgroundJobs.Interfaces;
using aqua_api.Infrastructure.Startup;

var builder = WebApplication.CreateBuilder(args);

// Load local overrides only in Development.
// Production should rely on appsettings.Production.json and environment variables.
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
}

var configuredCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?.Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.Trim().TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray()
    ?? Array.Empty<string>();

if (configuredCorsOrigins.Length == 0)
{
    throw new InvalidOperationException("Cors:AllowedOrigins ayari bos birakilamaz.");
}

// Add services to the container.
builder.Services.AddControllers();

// Centralized validation response
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var localization = context.HttpContext.RequestServices.GetRequiredService<aqua_api.Interfaces.ILocalizationService>();
        var errors = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        var response = aqua_api.DTOs.ApiResponse<object>.ErrorResult(
            localization.GetLocalizedString("General.ValidationError"),
            localization.GetLocalizedString("General.ValidationError"),
            StatusCodes.Status400BadRequest);
        response.Errors = errors;
        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
    };
});


builder.Services.AddMemoryCache();
var dataProtectionKeyPath =
    builder.Configuration["DataProtection:KeyPath"] ??
    Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionKeyPath);
builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyPath))
    .SetApplicationName("V3RII_AQUA");

// SignalR Configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

// Entity Framework Configuration - Using SQL Server
builder.Services.AddDbContext<AquaDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60);
    });
});

// Hangfire Configuration
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.Configure<HangfireMonitoringOptions>(
    builder.Configuration.GetSection(HangfireMonitoringOptions.SectionName));

GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
{
    Attempts = 3,
    DelaysInSeconds = new[] { 60, 300, 900 },
    LogEvents = true,
    OnAttemptsExceeded = AttemptsExceededAction.Fail
});

builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "default", "dead-letter" };
});

// Creates the first admin user only when the DB is empty and BootstrapAdmin is configured.
builder.Services.AddHostedService<AdminBootstrapHostedService>();

// ERP Database Configuration - Using SQL Server
builder.Services.AddDbContext<ErpAquaDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("ErpConnection");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60);
    });
});

// AutoMapper Configuration - Automatically discover all mapping profiles in the assembly
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Register Core Services
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IBalanceLedgerManager, BalanceLedgerManager>();
builder.Services.AddScoped<ITransferRepository, TransferRepository>();
builder.Services.AddScoped<IMortalityRepository, MortalityRepository>();
builder.Services.AddScoped<IWeighingRepository, WeighingRepository>();
builder.Services.AddScoped<IStockConvertRepository, StockConvertRepository>();
builder.Services.AddScoped<INetOperationRepository, NetOperationRepository>();
builder.Services.AddScoped<IDailyWeatherRepository, DailyWeatherRepository>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();

// Register Authentication & Authorization Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserAuthorityService, UserAuthorityService>();

// Register Localization Services
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

// Register ERP Service
builder.Services.AddScoped<IErpService, ErpService>();

// Register User Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPermissionAccessService, PermissionAccessService>();
builder.Services.AddScoped<IPermissionDefinitionService, PermissionDefinitionService>();
builder.Services.AddScoped<IPermissionGroupService, PermissionGroupService>();
builder.Services.AddScoped<IUserPermissionGroupService, UserPermissionGroupService>();
builder.Services.AddScoped<IUserDetailService, UserDetailService>();

// Register Stock Services
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IStockDetailService, StockDetailService>();
builder.Services.AddScoped<IStockImageService, StockImageService>();
builder.Services.AddScoped<IStockRelationService, StockRelationService>();

// Register Aqua Entity Services
builder.Services.AddScoped<IBatchCageBalanceService, BatchCageBalanceService>();
builder.Services.AddScoped<IBatchMovementService, BatchMovementService>();
builder.Services.AddScoped<ICageService, CageService>();
builder.Services.AddScoped<IDailyWeatherService, DailyWeatherService>();
builder.Services.AddScoped<IFeedingService, FeedingService>();
builder.Services.AddScoped<IFeedingDistributionService, FeedingDistributionService>();
builder.Services.AddScoped<IFeedingLineService, FeedingLineService>();
builder.Services.AddScoped<IFishBatchService, FishBatchService>();
builder.Services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
builder.Services.AddScoped<IGoodsReceiptFishDistributionService, GoodsReceiptFishDistributionService>();
builder.Services.AddScoped<IGoodsReceiptLineService, GoodsReceiptLineService>();
builder.Services.AddScoped<IMortalityService, MortalityService>();
builder.Services.AddScoped<IMortalityLineService, MortalityLineService>();
builder.Services.AddScoped<INetOperationService, NetOperationService>();
builder.Services.AddScoped<INetOperationLineService, NetOperationLineService>();
builder.Services.AddScoped<INetOperationTypeService, NetOperationTypeService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectCageService, ProjectCageService>();
builder.Services.AddScoped<IStockConvertService, StockConvertService>();
builder.Services.AddScoped<IStockConvertLineService, StockConvertLineService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<ITransferLineService, TransferLineService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<IShipmentLineService, ShipmentLineService>();
builder.Services.AddScoped<IWeatherSeverityService, WeatherSeverityService>();
builder.Services.AddScoped<IWeatherTypeService, WeatherTypeService>();
builder.Services.AddScoped<IWeighingService, WeighingService>();
builder.Services.AddScoped<IWeighingLineService, WeighingLineService>();

// Register Mail Services
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<ISmtpSettingsService, SmtpSettingsService>();

// Register Background Jobs
builder.Services.AddScoped<Infrastructure.BackgroundJobs.Interfaces.IStockSyncJob, Infrastructure.BackgroundJobs.StockSyncJob>();
builder.Services.AddScoped<Infrastructure.BackgroundJobs.Interfaces.IMailJob, Infrastructure.BackgroundJobs.MailJob>();
builder.Services.AddScoped<Infrastructure.BackgroundJobs.Interfaces.IHangfireDeadLetterJob, Infrastructure.BackgroundJobs.HangfireDeadLetterJob>();

// Register File Upload Services
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// Add HttpContextAccessor for accessing HTTP context in services
builder.Services.AddHttpContextAccessor();

// Add HttpClient for external requests (e.g., image loading in PDF generation)
builder.Services.AddHttpClient();

// Localization Configuration
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Request Localization Configuration
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("tr-TR"),
        new CultureInfo("de-DE"),
        new CultureInfo("fr-FR"),
        new CultureInfo("es-ES"),
        new CultureInfo("it-IT")
    };

    options.DefaultRequestCulture = new RequestCulture("tr-TR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Add custom request culture provider for x-language header
    options.RequestCultureProviders.Insert(0, new CustomHeaderRequestCultureProvider());
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins(configuredCorsOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// JWT Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;

    var jwtSecret = builder.Configuration["JwtSettings:SecretKey"];
    if (string.IsNullOrWhiteSpace(jwtSecret))
    {
        throw new InvalidOperationException("JwtSettings:SecretKey is required.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "CmsWebApi",
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "CmsWebApiUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && (
                path.StartsWithSegments("/api/authHub") ||
                path.StartsWithSegments("/authHub") ||
                path.StartsWithSegments("/api/notificationHub") ||
                path.StartsWithSegments("/notificationHub")))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<AquaDbContext>();
            var claims = context.Principal?.Claims;
            var userId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                context.Fail("Token geçersiz: eksik kullanıcı ID");
                return;
            }

            var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            var accessToken = context.HttpContext.Request.Query["access_token"].FirstOrDefault();

            string? rawToken = null;
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                rawToken = authHeader.Substring("Bearer ".Length).Trim();
            }
            else if (!string.IsNullOrEmpty(accessToken))
            {
                rawToken = accessToken;
            }

            string? tokenHash = null;
            if (!string.IsNullOrEmpty(rawToken))
            {
                using var sha256Hash = System.Security.Cryptography.SHA256.Create();
                var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
                var builderStr = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builderStr.Append(bytes[i].ToString("x2"));
                }
                tokenHash = builderStr.ToString();
            }

            try
            {
                var session = await db.UserSessions
                    .AsNoTracking()
                    .Where(s => s.UserId.ToString() == userId
                        && s.RevokedAt == null
                        && (tokenHash != null && s.Token == tokenHash))
                    .FirstOrDefaultAsync(context.HttpContext.RequestAborted);

                if (session == null)
                {
                    context.Fail("Token geçersiz veya oturum kapandı");
                }
            }
            catch (Exception ex)
            {
                context.Fail($"Session kontrolü sırasında hata: {ex.Message}");
            }
        }
    };
});

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CRM Web API",
        Version = "v1",
        Description = "A comprehensive CRM Web API with JWT Authentication",
        Contact = new OpenApiContact
        {
            Name = "CRM API Team",
            Email = "support@crmapi.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityDefinition("Language", new OpenApiSecurityScheme
    {
        Description = "Language header for localization. Use 'tr' for Turkish or 'en' for English. Example: \"x-language: tr\"",
        Name = "x-language",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKey"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Language"
                }
            },
            new List<string>()
        }
    });

    c.CustomSchemaIds(type => type.FullName);

    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

    c.ParameterFilter<FileUploadParameterFilter>();
    c.OperationFilter<FileUploadOperationFilter>();

    c.CustomOperationIds(apiDesc => apiDesc.ActionDescriptor.RouteValues["action"]);

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

GlobalJobFilters.Filters.Add(
    new HangfireJobStateFilter(
        app.Services.GetRequiredService<ILogger<HangfireJobStateFilter>>(),
        app.Services.GetRequiredService<IBackgroundJobClient>(),
        app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<HangfireMonitoringOptions>>(),
        app.Services.GetRequiredService<IServiceScopeFactory>()));

// Migrations are intentionally run out-of-band (e.g., dotnet ef database update)

// Configure the HTTP request pipeline.

// ── Early CORS middleware ──────────────────────────────────────────────
// Handles preflight (OPTIONS) requests *before* any other middleware can
// short-circuit. For non-preflight requests it adds the CORS headers so
// that even 500 / exception-handler responses carry them.
var allowedCorsOrigins = new HashSet<string>(configuredCorsOrigins, StringComparer.OrdinalIgnoreCase);

app.Use(async (ctx, next) =>
{
    var origin = ctx.Request.Headers["Origin"].ToString();
    if (!string.IsNullOrEmpty(origin) && allowedCorsOrigins.Contains(origin))
    {
        ctx.Response.Headers.Append("Access-Control-Allow-Origin", origin);
        ctx.Response.Headers.Append("Access-Control-Allow-Credentials", "true");

        // Preflight
        if (HttpMethods.IsOptions(ctx.Request.Method))
        {
            ctx.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, PATCH, DELETE, OPTIONS");
            ctx.Response.Headers.Append("Access-Control-Allow-Headers",
                "Content-Type, Authorization, X-Branch-Code, Branch-Code, X-Language, x-language, x-branch-code");
            ctx.Response.Headers.Append("Access-Control-Max-Age", "86400");
            ctx.Response.StatusCode = 204;
            return; // short-circuit – don't call next
        }
    }

    await next();
});

// Ensure 500 from unhandled exceptions still get CORS headers (browser would otherwise hide the response)
app.UseExceptionHandler(errApp =>
{
    errApp.Run(async ctx =>
    {
        var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
        var logger = ctx.RequestServices.GetService<ILogger<Program>>();
        var localizationService = ctx.RequestServices.GetService<ILocalizationService>();
        if (ex != null)
            logger?.LogError(ex, "Unhandled exception: {Path}", ctx.Request.Path);

        var dbUpdateException = FindDbUpdateException(ex);
        if (dbUpdateException != null &&
            DbUpdateExceptionHelper.TryGetUniqueViolation(dbUpdateException, out _))
        {
            var isCountryPath = ctx.Request.Path.StartsWithSegments("/api/Country", StringComparison.OrdinalIgnoreCase);
            var localizedMessage = isCountryPath
                ? (localizationService?.GetLocalizedString("CountryService.CountryNameAlreadyExists")
                   ?? "Country already exists. Duplicate country entries are not allowed.")
                : (localizationService?.GetLocalizedString("General.RecordAlreadyExists")
                   ?? "This record already exists.");

            var response = aqua_api.DTOs.ApiResponse<object>.ErrorResult(
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

        ctx.Response.StatusCode = 500;
        ctx.Response.ContentType = "application/json";
        var origin = ctx.Request.Headers["Origin"].ToString();
        if (!string.IsNullOrEmpty(origin) && allowedCorsOrigins.Contains(origin))
        {
            // Headers may already be set by the early middleware, but Append is
            // safe – duplicates are ignored when the value already exists.
            if (!ctx.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
            {
                ctx.Response.Headers.Append("Access-Control-Allow-Origin", origin);
                ctx.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
            }
        }
        var message = ex?.Message ?? "An error occurred.";
        var json = System.Text.Json.JsonSerializer.Serialize(new { error = "An error occurred.", message });
        await ctx.Response.WriteAsync(json);
    });
});

static DbUpdateException? FindDbUpdateException(Exception? exception)
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

app.UseRouting();

app.UseCors("DevCors");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM Web API v1");
        c.RoutePrefix = "swagger";
    });
}

// Static files for uploaded images - wwwroot folder (default)
app.UseStaticFiles();

// Static files for uploads folder (project root/uploads)
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (Directory.Exists(uploadsPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads"
    });
}

// Add Request Localization Middleware
app.UseRequestLocalization();

// Add BranchCode Middleware
app.UseMiddleware<BranchCodeMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Endpoint mapping
app.MapHub<AuthHub>("/authHub");
app.MapHub<aqua_api.Hubs.NotificationHub>("/notificationHub");
app.MapControllers();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Register Recurring Jobs
// Development ortaminda varsayilan olarak kapali kalir.
// Test icin appsettings.Local.json -> Hangfire:StockSync:EnableInDevelopment = true yapilabilir.
var enableStockSyncInDevelopment = app.Configuration.GetValue<bool>("Hangfire:StockSync:EnableInDevelopment");
var shouldRunRecurringStockSync = !app.Environment.IsDevelopment() || enableStockSyncInDevelopment;

if (shouldRunRecurringStockSync)
{
    RecurringJob.AddOrUpdate<IStockSyncJob>(
        "erp-stock-sync-job",
        job => job.ExecuteAsync(),
        Cron.MinuteInterval(30));
}
else
{
    RecurringJob.RemoveIfExists("erp-stock-sync-job");
    app.Logger.LogInformation("Skipping recurring ERP sync jobs in Development environment. Set Hangfire:StockSync:EnableInDevelopment=true to enable.");
}

app.Run();

public partial class Program { }
