using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using aqua_api.Modules.Identity.Domain.Entities;
using aqua_api.Modules.Integrations.Application.Dtos;
using aqua_api.Modules.Integrations.Application.Services;
using aqua_api.Modules.Stock.Domain.Entities;
using aqua_api.Modules.Warehouse.Domain.Entities;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Xunit;

namespace aqua_api.Tests;

public sealed class AquaHttpTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
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
        await _connection.OpenAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedMasterDataAsync(db);
    }

    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        Dispose();
    }

    public static async Task SeedMasterDataAsync(AquaDbContext db)
    {
        if (await db.Users.AnyAsync(x => x.Id == 1))
        {
            return;
        }

        db.Users.Add(new User
        {
            Id = 1,
            Username = "integration-user",
            Email = "integration-user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!"),
            FirstName = "Integration",
            LastName = "User",
            RoleId = 1,
            IsActive = true,
            IsEmailConfirmed = true,
        });

        db.Stocks.AddRange(
            new Stock
            {
                ErpStockCode = "PLAMUT-5G",
                StockName = "Plamut 5g",
                Unit = "ADET",
                BranchCode = 1,
            },
            new Stock
            {
                ErpStockCode = "PLAMUT-10G",
                StockName = "Plamut 10g",
                Unit = "ADET",
                BranchCode = 1,
            },
            new Stock
            {
                ErpStockCode = "YEM-STD",
                StockName = "Standart Yem",
                Unit = "KG",
                BranchCode = 1,
            });

        db.Warehouses.Add(new Warehouse
        {
            ErpWarehouseCode = 10,
            WarehouseName = "Ana Depo",
            BranchCode = 1,
            AllowNegativeBalance = false,
            IsLocked = false,
        });

        await db.SaveChangesAsync();
    }

    public static async Task SeedFeedPurchaseHistoryAsync(AquaDbContext db, long projectId, long warehouseId, long feedStockId)
    {
        if (await db.GoodsReceipts.AnyAsync(x => !x.IsDeleted && x.ReceiptNo == "FEED-01"))
        {
            return;
        }

        var rows = new[]
        {
            new { ReceiptNo = "FEED-01", Date = new DateTime(2026, 4, 1), TotalGram = 20_000m, LocalAmount = 1_160m },
            new { ReceiptNo = "FEED-02", Date = new DateTime(2026, 4, 2), TotalGram = 25_000m, LocalAmount = 1_500m },
            new { ReceiptNo = "FEED-03", Date = new DateTime(2026, 4, 4), TotalGram = 18_000m, LocalAmount = 1_188m },
        };

        foreach (var row in rows)
        {
            var header = new aqua_api.Modules.Aqua.Domain.Entities.GoodsReceipt
            {
                ProjectId = projectId,
                ReceiptNo = row.ReceiptNo,
                ReceiptDate = row.Date,
                Status = aqua_api.Modules.Aqua.Domain.Enums.DocumentStatus.Posted,
                WarehouseId = warehouseId,
            };

            db.GoodsReceipts.Add(header);
            await db.SaveChangesAsync();

            db.GoodsReceiptLines.Add(new aqua_api.Modules.Aqua.Domain.Entities.GoodsReceiptLine
            {
                GoodsReceiptId = header.Id,
                ItemType = aqua_api.Modules.Aqua.Domain.Enums.GoodsReceiptItemType.Feed,
                StockId = feedStockId,
                QtyUnit = 1,
                GramPerUnit = row.TotalGram,
                TotalGram = row.TotalGram,
                CurrencyCode = "TRY",
                ExchangeRate = 1m,
                UnitPrice = row.LocalAmount / (row.TotalGram / 1000m),
                LocalUnitPrice = row.LocalAmount / (row.TotalGram / 1000m),
                LineAmount = row.LocalAmount,
                LocalLineAmount = row.LocalAmount,
            });
        }

        await db.SaveChangesAsync();
    }
}

internal sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "integration-user"),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

internal sealed class FakeErpService : IErpService
{
    public Task<ApiResponse<short>> GetBranchCodeFromContext()
        => Task.FromResult(ApiResponse<short>.SuccessResult(1, "ok"));

    public Task<ApiResponse<List<CariDto>>> GetCarisAsync(string? cariKodu)
        => Task.FromResult(ApiResponse<List<CariDto>>.SuccessResult([], "ok"));

    public Task<ApiResponse<List<CariDto>>> GetCarisByCodesAsync(IEnumerable<string> cariKodlari)
        => Task.FromResult(ApiResponse<List<CariDto>>.SuccessResult([], "ok"));

    public Task<ApiResponse<List<DepoDto>>> GetDeposAsync(short? depoKodu)
        => Task.FromResult(ApiResponse<List<DepoDto>>.SuccessResult([], "ok"));

    public Task<ApiResponse<List<StokFunctionDto>>> GetStoksAsync(string? stokKodu)
        => Task.FromResult(ApiResponse<List<StokFunctionDto>>.SuccessResult([], "ok"));

    public Task<ApiResponse<List<BranchDto>>> GetBranchesAsync(int? branchNo = null)
        => Task.FromResult(ApiResponse<List<BranchDto>>.SuccessResult([], "ok"));

    public Task<ApiResponse<List<KurDto>>> GetExchangeRateAsync(DateTime tarih, int fiyatTipi)
        => Task.FromResult(ApiResponse<List<KurDto>>.SuccessResult([], "ok"));

    public Task<ApiResponse<List<ErpShippingAddressDto>>> GetErpShippingAddressAsync(string customerCode)
        => Task.FromResult(ApiResponse<List<ErpShippingAddressDto>>.SuccessResult([], "ok"));

    public Task<ApiResponse<List<StokGroupDto>>> GetStokGroupAsync(string? grupKodu)
        => Task.FromResult(ApiResponse<List<StokGroupDto>>.SuccessResult([], "ok"));

    public Task<ApiResponse<List<ProjeDto>>> GetProjectCodesAsync()
        => Task.FromResult(ApiResponse<List<ProjeDto>>.SuccessResult([], "ok"));

    public Task<ApiResponse<object>> HealthCheckAsync()
        => Task.FromResult(ApiResponse<object>.SuccessResult(new { healthy = true }, "ok"));
}

internal sealed class SqliteHttpTestAquaDbContext : AquaDbContext
{
    public SqliteHttpTestAquaDbContext(DbContextOptions<AquaDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var columnType = property.GetColumnType();
                if (!string.IsNullOrWhiteSpace(columnType) && columnType.Contains("max", StringComparison.OrdinalIgnoreCase))
                {
                    property.SetColumnType("TEXT");
                }
            }
        }

        var feedingEntity = modelBuilder.Model.FindEntityType(typeof(aqua_api.Modules.Aqua.Domain.Entities.Feeding));
        var feedingDateOnly = feedingEntity?.FindProperty("FeedingDateOnly");
        feedingDateOnly?.SetAnnotation("Relational:ComputedColumnSql", "date(FeedingDate)");
        feedingDateOnly?.SetAnnotation("Relational:IsStored", true);
    }
}
