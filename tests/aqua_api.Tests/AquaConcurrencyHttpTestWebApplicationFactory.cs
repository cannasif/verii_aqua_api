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

public sealed class AquaConcurrencyHttpTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"aqua-concurrency-{Guid.NewGuid():N}.db");

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
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=AquaTests;Trusted_Connection=True;",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AquaDbContext>));
            services.RemoveAll(typeof(AquaDbContext));
            services.RemoveAll(typeof(IErpService));

            services.AddScoped<AquaDbContext>(_ =>
            {
                var options = new DbContextOptionsBuilder<AquaDbContext>()
                    .UseSqlite($"Data Source={_dbPath}")
                    .EnableSensitiveDataLogging()
                    .Options;
                return new SqliteHttpTestAquaDbContext(options);
            });

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
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        await AquaHttpTestWebApplicationFactory.SeedMasterDataAsync(db);
    }

    public new Task DisposeAsync()
    {
        try
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
        catch
        {
        }

        Dispose();
        return Task.CompletedTask;
    }
}
