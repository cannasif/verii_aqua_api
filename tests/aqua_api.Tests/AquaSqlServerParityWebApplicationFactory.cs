using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using aqua_api.Modules.Integrations.Application.Services;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Xunit;

namespace aqua_api.Tests;

public sealed class AquaSqlServerParityWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";
    private const string SaPassword = "ParityPass_123!";

    private string? _connectionString;
    private string? _containerName;
    private int _hostPort;

    public string? UnavailableReason { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "http://localhost",
                ["JwtSettings:SecretKey"] = "TEST_SUPER_SECRET_KEY_12345678901234567890",
                ["JwtSettings:Issuer"] = "AquaTestIssuer",
                ["JwtSettings:Audience"] = "AquaTestAudience",
                ["ConnectionStrings:DefaultConnection"] = _connectionString ?? "Server=127.0.0.1,1433;Database=Unavailable;User Id=sa;Password=Unavailable123!;TrustServerCertificate=True;Encrypt=False;",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IErpService));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName,
                _ => { });

            services.AddScoped<IErpService, FakeErpService>();
        });
    }

    public async Task InitializeAsync()
    {
        if (!await CanUseDockerAsync())
        {
            return;
        }

        _hostPort = Random.Shared.Next(15433, 15999);
        _containerName = $"aqua-sql-parity-{Guid.NewGuid():N}";
        _connectionString = $"Server=127.0.0.1,{_hostPort};Database=AquaParity;User Id=sa;Password={SaPassword};TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True;";

        var runResult = await RunProcessAsync("docker", $"run --rm -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD={SaPassword} -p {_hostPort}:1433 --name {_containerName} -d {SqlServerImage}");
        if (runResult.ExitCode != 0)
        {
            UnavailableReason = $"SQL Server container could not start: {runResult.Error}";
            return;
        }

        var ready = await WaitForSqlServerAsync(_containerName);
        if (!ready)
        {
            UnavailableReason = "SQL Server container started but did not become ready in time.";
            return;
        }

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
        await db.Database.EnsureCreatedAsync();
        await AquaHttpTestWebApplicationFactory.SeedMasterDataAsync(db);
    }

    public new async Task DisposeAsync()
    {
        if (!string.IsNullOrWhiteSpace(_containerName))
        {
            await RunProcessAsync("docker", $"rm -f {_containerName}");
        }

        Dispose();
    }

    private async Task<bool> CanUseDockerAsync()
    {
        var dockerInfo = await RunProcessAsync("docker", "info");
        if (dockerInfo.ExitCode != 0)
        {
            UnavailableReason = "Docker daemon is not running.";
            return false;
        }

        var imageInspect = await RunProcessAsync("docker", $"image inspect {SqlServerImage}");
        if (imageInspect.ExitCode != 0)
        {
            UnavailableReason = $"Required SQL Server image is not available locally: {SqlServerImage}";
            return false;
        }

        return true;
    }

    private static async Task<bool> WaitForSqlServerAsync(string containerName)
    {
        for (var i = 0; i < 30; i++)
        {
            var probe = await RunProcessAsync(
                "docker",
                $"exec {containerName} /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P {SaPassword} -C -Q \"SELECT 1\"");

            if (probe.ExitCode == 0)
            {
                return true;
            }

            await Task.Delay(2000);
        }

        return false;
    }

    private static async Task<ProcessResult> RunProcessAsync(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return new ProcessResult(
            process.ExitCode,
            (await stdoutTask).Trim(),
            (await stderrTask).Trim());
    }

    private sealed record ProcessResult(int ExitCode, string Output, string Error);
}
