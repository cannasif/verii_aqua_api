using System.Globalization;
using System.Security.Claims;
using System.Text;
using aqua_api.Modules.Integrations.Infrastructure.Auth;
using aqua_api.Modules.Integrations.Infrastructure.Clients;
using aqua_api.Modules.Integrations.Infrastructure.Options;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using aqua_api.Shared.Host.WebApi.Hubs;
using aqua_api.Modules.System.Infrastructure.Startup;
using aqua_api.Shared.Host.WebApi.Routing;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace aqua_api.Shared.Host.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAquaApiWebApi(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        string[] configuredCorsOrigins)
    {
        var isTesting = environment.IsEnvironment("Testing");

        if (configuredCorsOrigins.Length == 0)
        {
            throw new InvalidOperationException(LocalizationBootstrap.GetString("General.CorsAllowedOriginsRequired"));
        }
        var allowedCorsOrigins = CorsOriginMatcher.NormalizeAllowedOrigins(configuredCorsOrigins);

        services.AddControllers(options =>
        {
            options.Conventions.Add(new IisSafeHttpMethodConvention());
        });

        services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var localization = context.HttpContext.RequestServices.GetRequiredService<ILocalizationService>();
                var errors = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => localization.GetLocalizedString(e.ErrorMessage))
                    .ToList();
                var response = ApiResponse<object>.ErrorResult(
                    localization.GetLocalizedString("General.ValidationError"),
                    localization.GetLocalizedString("General.ValidationError"),
                    StatusCodes.Status400BadRequest);
                response.Errors = errors;
                return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
            };
        });

        services.AddMemoryCache();

        var dataProtectionKeyPath =
            configuration["DataProtection:KeyPath"] ??
            Path.Combine(environment.ContentRootPath, "DataProtectionKeys");
        Directory.CreateDirectory(dataProtectionKeyPath);

        services
            .AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyPath))
            .SetApplicationName("V3RII_AQUA");

        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });

        services.AddDbContext<AquaDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.CommandTimeout(60);
                sqlOptions.UseCompatibilityLevel(120);
            });
        });

        services.Configure<HangfireMonitoringOptions>(
            configuration.GetSection(HangfireMonitoringOptions.SectionName));
        services.Configure<NetsisOptions>(
            configuration.GetSection(NetsisOptions.SectionName));
        services.PostConfigure<NetsisOptions>(options =>
        {
            var legacySection = configuration.GetSection("NetsisRest");
            if (!legacySection.Exists())
            {
                return;
            }

            options.Enabled = options.Enabled || legacySection.GetValue<bool>("Enabled");
            options.Rest.BaseUrl = string.IsNullOrWhiteSpace(options.Rest.BaseUrl)
                ? legacySection.GetValue<string>("BaseUrl") ?? string.Empty
                : options.Rest.BaseUrl;
            options.Rest.LoginPath = string.IsNullOrWhiteSpace(options.Rest.LoginPath)
                ? legacySection.GetValue<string>("LoginPath") ?? string.Empty
                : options.Rest.LoginPath;
            options.Rest.ItemSlipsPath = string.IsNullOrWhiteSpace(options.Rest.ItemSlipsPath)
                ? legacySection.GetValue<string>("ItemSlipsPath") ?? "/api/v2/ItemSlips"
                : options.Rest.ItemSlipsPath;
            options.Rest.ItemsPath = string.IsNullOrWhiteSpace(options.Rest.ItemsPath)
                ? legacySection.GetValue<string>("ItemsPath") ?? string.Empty
                : options.Rest.ItemsPath;
            options.Rest.ArpsPath = string.IsNullOrWhiteSpace(options.Rest.ArpsPath)
                ? legacySection.GetValue<string>("ArpsPath") ?? string.Empty
                : options.Rest.ArpsPath;
            options.Rest.WarehouseTransferInDocumentType = options.Rest.WarehouseTransferInDocumentType > 0
                ? options.Rest.WarehouseTransferInDocumentType
                : legacySection.GetValue<int?>("WarehouseTransferInDocumentType") ?? 8;
            options.Rest.WarehouseTransferOutDocumentType = options.Rest.WarehouseTransferOutDocumentType > 0
                ? options.Rest.WarehouseTransferOutDocumentType
                : legacySection.GetValue<int?>("WarehouseTransferOutDocumentType") ?? 9;
            options.Rest.FeedWarehouseTransferOutSeries = string.IsNullOrWhiteSpace(options.Rest.FeedWarehouseTransferOutSeries)
                ? legacySection.GetValue<string>("FeedWarehouseTransferOutSeries") ?? "YEM"
                : options.Rest.FeedWarehouseTransferOutSeries;
            options.Rest.MortalityWarehouseTransferOutSeries = string.IsNullOrWhiteSpace(options.Rest.MortalityWarehouseTransferOutSeries)
                ? legacySection.GetValue<string>("MortalityWarehouseTransferOutSeries") ?? "FIR"
                : options.Rest.MortalityWarehouseTransferOutSeries;
            options.Rest.WarehouseTransferOutExpenseCode = string.IsNullOrWhiteSpace(options.Rest.WarehouseTransferOutExpenseCode)
                ? legacySection.GetValue<string>("WarehouseTransferOutExpenseCode")
                : options.Rest.WarehouseTransferOutExpenseCode;
            options.Rest.FeedWarehouseTransferOutExpenseCode = string.IsNullOrWhiteSpace(options.Rest.FeedWarehouseTransferOutExpenseCode)
                ? legacySection.GetValue<string>("FeedWarehouseTransferOutExpenseCode")
                : options.Rest.FeedWarehouseTransferOutExpenseCode;
            options.Rest.MortalityWarehouseTransferOutExpenseCode = string.IsNullOrWhiteSpace(options.Rest.MortalityWarehouseTransferOutExpenseCode)
                ? legacySection.GetValue<string>("MortalityWarehouseTransferOutExpenseCode")
                : options.Rest.MortalityWarehouseTransferOutExpenseCode;
            options.Rest.FeedWarehouseTransferOutWarehouseCode ??= legacySection.GetValue<int?>("FeedWarehouseTransferOutWarehouseCode");
            options.Rest.UseRestGeneratedWarehouseTransferNumbers = legacySection.GetValue<bool?>("UseRestGeneratedWarehouseTransferNumbers")
                ?? options.Rest.UseRestGeneratedWarehouseTransferNumbers;
            options.Rest.DefaultWarehouseCode ??= legacySection.GetValue<int?>("DefaultWarehouseCode");
            options.Rest.TimeoutSeconds = options.Rest.TimeoutSeconds > 0
                ? options.Rest.TimeoutSeconds
                : legacySection.GetValue<int?>("TimeoutSeconds") ?? 30;
            options.Rest.AllowInvalidSslCertificate = options.Rest.AllowInvalidSslCertificate
                || legacySection.GetValue<bool>("AllowInvalidSslCertificate");
            options.Rest.DefaultTokenLifetimeMinutes = options.Rest.DefaultTokenLifetimeMinutes > 0
                ? options.Rest.DefaultTokenLifetimeMinutes
                : legacySection.GetValue<int?>("DefaultTokenLifetimeMinutes") ?? 60;
            options.Rest.TokenExpirySkewSeconds = options.Rest.TokenExpirySkewSeconds > 0
                ? options.Rest.TokenExpirySkewSeconds
                : legacySection.GetValue<int?>("TokenExpirySkewSeconds") ?? 30;
            options.Rest.Username = string.IsNullOrWhiteSpace(options.Rest.Username)
                ? legacySection.GetValue<string>("Username") ?? string.Empty
                : options.Rest.Username;
            options.Rest.Password = string.IsNullOrWhiteSpace(options.Rest.Password)
                ? legacySection.GetValue<string>("Password") ?? string.Empty
                : options.Rest.Password;
            options.Rest.BranchCode = string.IsNullOrWhiteSpace(options.Rest.BranchCode)
                ? legacySection.GetValue<string>("BranchCode") ?? string.Empty
                : options.Rest.BranchCode;
            options.Rest.DbName = string.IsNullOrWhiteSpace(options.Rest.DbName)
                ? legacySection.GetValue<string>("DbName")
                    ?? legacySection.GetValue<string>("Database")
                    ?? string.Empty
                : options.Rest.DbName;
            options.Rest.DbUser = string.IsNullOrWhiteSpace(options.Rest.DbUser)
                ? legacySection.GetValue<string>("DbUser") ?? string.Empty
                : options.Rest.DbUser;
            options.Rest.DbPassword = string.IsNullOrWhiteSpace(options.Rest.DbPassword)
                ? legacySection.GetValue<string>("DbPassword") ?? string.Empty
                : options.Rest.DbPassword;
            options.Rest.DbType = string.IsNullOrWhiteSpace(options.Rest.DbType)
                ? legacySection.GetValue<string>("DbType") ?? string.Empty
                : options.Rest.DbType;
        });

        if (!isTesting)
        {
            services.AddHangfire(hangfire => hangfire
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
            {
                Attempts = 3,
                DelaysInSeconds = new[] { 60, 300, 900 },
                LogEvents = true,
                OnAttemptsExceeded = AttemptsExceededAction.Fail
            });

            services.AddHangfireServer(options =>
            {
                options.Queues = new[] { "default", "dead-letter" };
            });

            services.AddHostedService<AdminBootstrapHostedService>();
        }
        services.AddAutoMapper(typeof(Program).Assembly);
        services.AddAquaApplicationModules();
        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services.AddHttpClient<INetsisRestClient, NetsisRestClient>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<NetsisOptions>>()
                .Value;

            if (!string.IsNullOrWhiteSpace(options.Rest.BaseUrl))
            {
                client.BaseAddress = new Uri(options.Rest.BaseUrl, UriKind.Absolute);
            }

            client.Timeout = TimeSpan.FromSeconds(options.Rest.TimeoutSeconds > 0 ? options.Rest.TimeoutSeconds : 30);
        })
        .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<NetsisOptions>>()
                .Value;

            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = options.Rest.AllowInvalidSslCertificate
                    ? HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    : null
            };
        });
        services.AddHttpClient<INetsisAuthTokenService, NetsisAuthTokenService>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<NetsisOptions>>()
                .Value;

            if (!string.IsNullOrWhiteSpace(options.Rest.BaseUrl))
            {
                client.BaseAddress = new Uri(options.Rest.BaseUrl, UriKind.Absolute);
            }

            client.Timeout = TimeSpan.FromSeconds(options.Rest.TimeoutSeconds > 0 ? options.Rest.TimeoutSeconds : 30);
        })
        .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<NetsisOptions>>()
                .Value;

            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = options.Rest.AllowInvalidSslCertificate
                    ? HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    : null
            };
        });
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("tr-TR"),
                new CultureInfo("de-DE"),
                new CultureInfo("fr-FR"),
                new CultureInfo("es-ES"),
                new CultureInfo("it-IT"),
                new CultureInfo("ar-SA")
            };

            options.DefaultRequestCulture = new RequestCulture("tr-TR");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.RequestCultureProviders.Insert(0, new CustomHeaderRequestCultureProvider());
        });

        services.AddCors(options =>
        {
            options.AddPolicy("DevCors", policy =>
            {
                policy.SetIsOriginAllowed(origin => CorsOriginMatcher.IsAllowed(origin, allowedCorsOrigins, default))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = !environment.IsDevelopment();
            options.SaveToken = true;

            var jwtSecret = configuration["JwtSettings:SecretKey"];
            if (string.IsNullOrWhiteSpace(jwtSecret))
            {
                throw new InvalidOperationException(LocalizationBootstrap.GetString("General.JwtSecretRequired"));
            }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JwtSettings:Issuer"] ?? "CmsWebApi",
                ValidAudience = configuration["JwtSettings:Audience"] ?? "CmsWebApiUsers",
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
                        var localizationService = context.HttpContext.RequestServices.GetRequiredService<ILocalizationService>();
                        context.Fail(localizationService.GetLocalizedString("General.InvalidTokenMissingUserId"));
                        return;
                    }

                    var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                    var accessToken = context.HttpContext.Request.Query["access_token"].FirstOrDefault();

                    string? rawToken = null;
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    {
                        rawToken = authHeader["Bearer ".Length..].Trim();
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
                        var builder = new StringBuilder();
                        for (var i = 0; i < bytes.Length; i++)
                        {
                            builder.Append(bytes[i].ToString("x2"));
                        }

                        tokenHash = builder.ToString();
                    }

                    try
                    {
                        var session = await db.UserSessions
                            .AsNoTracking()
                            .Where(s => s.UserId.ToString() == userId
                                && s.RevokedAt == null
                                && tokenHash != null
                                && s.Token == tokenHash)
                            .FirstOrDefaultAsync(context.HttpContext.RequestAborted);

                        if (session == null)
                        {
                            var localizationService = context.HttpContext.RequestServices.GetRequiredService<ILocalizationService>();
                            context.Fail(localizationService.GetLocalizedString("General.InvalidTokenOrSessionClosed"));
                        }
                    }
                    catch (Exception ex)
                    {
                        var localizationService = context.HttpContext.RequestServices.GetRequiredService<ILocalizationService>();
                        context.Fail(localizationService.GetLocalizedString("General.SessionValidationError", ex.Message));
                    }
                }
            };
        });

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Aqua Web API",
                Version = "v1",
                Description = "Aqua operations, reporting, integration, and administration API with JWT authentication",
                Contact = new OpenApiContact
                {
                    Name = "V3RII Aqua API Team",
                    Email = "support@v3rii.com"
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityDefinition("Language", new OpenApiSecurityScheme
            {
                Description = "Language header for localization. Use 'tr' for Turkish or 'en' for English. Example: \"x-language: tr\"",
                Name = "x-language",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKey"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            options.CustomSchemaIds(type => type.FullName);

            options.MapType<IFormFile>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            });

            options.ParameterFilter<FileUploadParameterFilter>();
            options.OperationFilter<FileUploadOperationFilter>();
            options.CustomOperationIds(apiDesc => apiDesc.ActionDescriptor.RouteValues["action"]);

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }
}
