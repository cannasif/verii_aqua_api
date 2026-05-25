using aqua_api.Shared.Host.WebApi.Extensions;

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
    ?? Array.Empty<string>();

var configuredCorsOriginPatterns = builder.Configuration
    .GetSection("Cors:AllowedOriginPatterns")
    .Get<string[]>()
    ?? Array.Empty<string>();

configuredCorsOrigins = CorsOriginMatcher
    .NormalizeAllowedOrigins(configuredCorsOrigins.Concat(configuredCorsOriginPatterns));

builder.Services.AddAquaApiWebApi(builder.Configuration, builder.Environment, configuredCorsOrigins);

var app = builder.Build();
app.UseAquaApiWebApi(configuredCorsOrigins);

var isEfDesignTime = AppDomain.CurrentDomain.GetAssemblies()
    .Any(x => string.Equals(x.GetName().Name, "Microsoft.EntityFrameworkCore.Design", StringComparison.Ordinal));

if (!isEfDesignTime)
{
    app.Run();
}

public partial class Program { }
