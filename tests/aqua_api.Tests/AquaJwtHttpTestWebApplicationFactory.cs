using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using aqua_api.Modules.Integrations.Application.Services;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Xunit;

namespace aqua_api.Tests;

public sealed class AquaJwtHttpTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

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

            services.AddSingleton(_connection);
            services.AddScoped<AquaDbContext>(sp =>
            {
                var connection = sp.GetRequiredService<SqliteConnection>();
                var options = new DbContextOptionsBuilder<AquaDbContext>()
                    .UseSqlite(connection)
                    .EnableSensitiveDataLogging()
                    .Options;
                return new SqliteHttpTestAquaDbContext(options);
            });

            services.AddScoped<IErpService, FakeErpService>();
        });
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
        await db.Database.EnsureCreatedAsync();
        await AquaHttpTestWebApplicationFactory.SeedMasterDataAsync(db);
    }

    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        Dispose();
    }
}
