using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using aqua_api.Modules.Aqua.Application.Services;
using aqua_api.Modules.Integrations.Application.Dtos;
using aqua_api.Modules.Stock.Domain.Entities;
using aqua_api.Modules.System.Infrastructure.BackgroundJobs;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using aqua_api.Shared.Infrastructure.Services;

namespace aqua_api.Tests;

public sealed class ErpReceiptShipmentMovementSyncJobIntegrationTests
{
    [Fact]
    public async Task FeedReceipt_IsMarkedProcessed_AndRepairsAStaleMirrorState()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AquaDbContext>().UseSqlite(connection).Options;
        await using var db = new SyncJobSqliteAquaDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var unitOfWork = new EfUnitOfWork(db, new HttpContextAccessor());
        var localization = new LocalizationService(NullLogger<LocalizationService>.Instance);
        var job = new ErpReceiptShipmentMovementSyncJob(
            null!,
            db,
            unitOfWork,
            new BalanceLedgerManager(unitOfWork, localization),
            localization,
            NullLogger<ErpReceiptShipmentMovementSyncJob>.Instance);

        db.Stocks.Add(new Stock
        {
            ErpStockCode = "YEM-001",
            StockName = "Test Yemi",
            Unit = "KG",
            GrupKodu = "YEM",
            BranchCode = 1
        });
        await db.SaveChangesAsync();

        var movement = new MalKabulVeSevkiyatDto
        {
            Tarih = new DateTime(2026, 7, 16, 2, 28, 0),
            FisNo = "YEM202600000001",
            StokKodu = "YEM-001",
            StokAdi = "Test Yemi",
            Miktar = 100,
            HareketTuru = "J",
            GcKodu = "G",
            GrupKodu = "YEM",
            IslemTuru = "Mal Kabul (Diğer Giriş)"
        };

        await job.ProcessMovementInCurrentTransactionAsync(movement);

        var mirror = await db.ErpReceiptShipmentMovements.SingleAsync();
        var receiptLine = await db.GoodsReceiptLines.SingleAsync();
        Assert.True(mirror.IsMatched);
        Assert.True(mirror.IsProcessed);
        Assert.Equal(receiptLine.Id, mirror.GoodsReceiptLineId);
        Assert.Null(mirror.ProcessError);

        mirror.IsProcessed = false;
        mirror.ProcessedAt = null;
        mirror.GoodsReceiptLineId = null;
        await db.SaveChangesAsync();

        await job.ProcessMovementInCurrentTransactionAsync(movement);

        mirror = await db.ErpReceiptShipmentMovements.SingleAsync();
        Assert.True(mirror.IsProcessed);
        Assert.Equal(receiptLine.Id, mirror.GoodsReceiptLineId);
        Assert.Equal(1, await db.GoodsReceiptLines.CountAsync());

        unitOfWork.Dispose();
    }

    private sealed class SyncJobSqliteAquaDbContext(DbContextOptions<AquaDbContext> options) : AquaDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(x => x.GetProperties()))
            {
                var columnType = property.GetColumnType();
                if (!string.IsNullOrWhiteSpace(columnType) && columnType.Contains("max", StringComparison.OrdinalIgnoreCase))
                    property.SetColumnType("TEXT");
            }
        }
    }
}
