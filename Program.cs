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
    ?.Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.Trim().TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray()
    ?? Array.Empty<string>();

builder.Services.AddAquaApiWebApi(builder.Configuration, builder.Environment, configuredCorsOrigins);

var app = builder.Build();
app.UseAquaApiWebApi(configuredCorsOrigins);

app.Run();

public partial class Program { }
