using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using aqua_api.Modules.Aqua.Application.Services;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.Integrations.Application.Dtos;
using aqua_api.Modules.Integrations.Application.Services;
using aqua_api.Modules.Stock.Domain.Entities;
using aqua_api.Modules.System.Infrastructure.BackgroundJobs;
using aqua_api.Shared.Common.Dtos;
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
            new aqua_api.Modules.FishGrowths.Application.Services.FishGrowthLedgerReplayService(unitOfWork, localization),
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

    [Fact]
    public async Task FishReceiptAndShipment_UseCageGram_AndRemainAlignedWithLedgerReport()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AquaDbContext>().UseSqlite(connection).Options;
        await using var db = new SyncJobSqliteAquaDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var unitOfWork = new EfUnitOfWork(db, new HttpContextAccessor());
        var localization = new LocalizationService(NullLogger<LocalizationService>.Instance);
        var ledger = new BalanceLedgerManager(unitOfWork, localization);
        var job = new ErpReceiptShipmentMovementSyncJob(
            null!,
            db,
            unitOfWork,
            ledger,
            new aqua_api.Modules.FishGrowths.Application.Services.FishGrowthLedgerReplayService(unitOfWork, localization),
            localization,
            NullLogger<ErpReceiptShipmentMovementSyncJob>.Instance);
        var reportService = new DevirFcrReportService(unitOfWork, localization);

        var openingDate = new DateTime(2026, 1, 1);
        var project = new Project
        {
            ProjectCode = "ERP-FISH-001",
            ProjectName = "ERP Fish Project",
            StartDate = openingDate,
            Status = DocumentStatus.Posted
        };
        var cage = new Cage { CageCode = "B3", CageName = "B3 Cage" };
        var warehouse = new aqua_api.Modules.Warehouse.Domain.Entities.Warehouse
        {
            ErpWarehouseCode = 110,
            WarehouseName = "B3",
            BranchCode = 1
        };
        var stock = new Stock
        {
            ErpStockCode = "L001",
            StockName = "Levrek",
            Unit = "ADET",
            GrupKodu = "BALIK",
            BranchCode = 1
        };
        db.AddRange(project, cage, warehouse, stock);
        await db.SaveChangesAsync();

        db.CageWarehouseMappings.Add(new CageWarehouseMapping
        {
            CageId = cage.Id,
            WarehouseId = warehouse.Id,
            IsActive = true
        });
        var projectCage = new ProjectCage
        {
            ProjectId = project.Id,
            CageId = cage.Id,
            AssignedDate = openingDate
        };
        db.ProjectCages.Add(projectCage);
        await db.SaveChangesAsync();

        var batch = new FishBatch
        {
            ProjectId = project.Id,
            BatchCode = "BATCH-ERP-001",
            FishStockId = stock.Id,
            CurrentAverageGram = 150m,
            StartDate = openingDate
        };
        db.FishBatches.Add(batch);
        await db.SaveChangesAsync();

        await ledger.ApplyDelta(
            project.Id,
            batch.Id,
            projectCage.Id,
            1_000,
            200_000m,
            BatchMovementType.OpeningImport,
            openingDate,
            "Test opening",
            "TEST_OPENING",
            1,
            null,
            projectCage.Id,
            null,
            stock.Id,
            null,
            200m);
        await db.SaveChangesAsync();

        // The batch-wide value may differ when the same batch exists in multiple locations.
        // ERP location movements must use the selected cage's live gram value.
        batch.CurrentAverageGram = 150m;
        await db.SaveChangesAsync();

        var receiptMovement = new MalKabulVeSevkiyatDto
        {
            Tarih = openingDate.AddDays(1),
            FisNo = "ERP-GR-001",
            KafesKodu = 110,
            ProjeKodu = project.ProjectCode,
            StokKodu = stock.ErpStockCode,
            StokAdi = stock.StockName,
            Miktar = 100,
            HareketTuru = "J",
            GcKodu = "G",
            GrupKodu = "BALIK",
            IslemTuru = "Mal Kabul (Balik Girisi)"
        };
        var shipmentMovement = new MalKabulVeSevkiyatDto
        {
            Tarih = openingDate.AddDays(2),
            FisNo = "ERP-SH-001",
            KafesKodu = 110,
            ProjeKodu = project.ProjectCode,
            StokKodu = stock.ErpStockCode,
            StokAdi = stock.StockName,
            Miktar = 300,
            HareketTuru = "J",
            GcKodu = "C",
            GrupKodu = "BALIK",
            IslemTuru = "Sevkiyat"
        };

        await job.ProcessMovementInCurrentTransactionAsync(receiptMovement, "ERP-FISH-RECEIPT:1");
        await job.ProcessMovementInCurrentTransactionAsync(shipmentMovement, "ERP-FISH-SHIPMENT:1");

        var receiptLine = await db.GoodsReceiptLines.SingleAsync(x => x.ErpSourceMovementKey == "ERP-FISH-RECEIPT:1");
        var shipmentLine = await db.ShipmentLines.SingleAsync(x => x.ErpSourceMovementKey == "ERP-FISH-SHIPMENT:1");
        var balance = await db.BatchCageBalances.SingleAsync(x => x.FishBatchId == batch.Id && x.ProjectCageId == projectCage.Id);

        Assert.Equal(200m, receiptLine.FishAverageGram);
        Assert.Equal(20_000m, receiptLine.FishTotalGram);
        Assert.Equal(200m, shipmentLine.AverageGram);
        Assert.Equal(60_000m, shipmentLine.BiomassGram);
        Assert.Equal(800, balance.LiveCount);
        Assert.Equal(160_000m, balance.BiomassGram);
        Assert.Equal(200m, balance.AverageGram);

        var reportResponse = await reportService.GetReportAsync(new DevirFcrReportRequestDto
        {
            ProjectIds = new List<long> { project.Id },
            FromDate = openingDate,
            ToDate = openingDate.AddDays(2)
        });

        Assert.True(reportResponse.Success, reportResponse.ExceptionMessage);
        var report = Assert.Single(reportResponse.Data!.Rows);
        Assert.Equal(1_000, report.OpeningFishCount);
        Assert.Equal(300, report.ShipmentFishCount);
        Assert.Equal(800, report.EndingFishCount);
        Assert.Equal(200m, report.EndingAverageGram);
        Assert.Equal(200m, report.OpeningBiomassKg);
        Assert.Equal(160m, report.EndingBiomassKg);
        Assert.Equal(60m, report.ShippedBiomassKg);
        Assert.Equal(0m, report.MortalityBiomassKg);
        Assert.Equal(0m, report.MortalityPct);

        await job.ProcessMovementInCurrentTransactionAsync(receiptMovement, "ERP-FISH-RECEIPT:1");
        await job.ProcessMovementInCurrentTransactionAsync(shipmentMovement, "ERP-FISH-SHIPMENT:1");

        Assert.Equal(1, await db.GoodsReceiptLines.CountAsync(x => x.ErpSourceMovementKey == "ERP-FISH-RECEIPT:1"));
        Assert.Equal(1, await db.ShipmentLines.CountAsync(x => x.ErpSourceMovementKey == "ERP-FISH-SHIPMENT:1"));
        Assert.Equal(3, await db.BatchMovements.CountAsync(x => x.FishBatchId == batch.Id));
        Assert.Equal(2, await db.ErpReceiptShipmentMovements.CountAsync(x => x.IsProcessed && !x.IsDeleted));

        unitOfWork.Dispose();
    }

    [Fact]
    public async Task NewFishReceipt_UsesOptionalErpGram_AndFallsBackToOneGram()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AquaDbContext>().UseSqlite(connection).Options;
        await using var db = new SyncJobSqliteAquaDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var unitOfWork = new EfUnitOfWork(db, new HttpContextAccessor());
        var localization = new LocalizationService(NullLogger<LocalizationService>.Instance);
        var ledger = new BalanceLedgerManager(unitOfWork, localization);
        var job = new ErpReceiptShipmentMovementSyncJob(
            null!,
            db,
            unitOfWork,
            ledger,
            new aqua_api.Modules.FishGrowths.Application.Services.FishGrowthLedgerReplayService(unitOfWork, localization),
            localization,
            NullLogger<ErpReceiptShipmentMovementSyncJob>.Instance);
        var reportService = new DevirFcrReportService(unitOfWork, localization);

        var stock = new Stock
        {
            ErpStockCode = "L001",
            StockName = "Levrek",
            Unit = "ADET",
            GrupKodu = "BALIK",
            BranchCode = 1
        };
        var cageWithGram = new Cage { CageCode = "B3", CageName = "B3 Cage" };
        var cageWithoutGram = new Cage { CageCode = "B4", CageName = "B4 Cage" };
        var warehouseWithGram = new aqua_api.Modules.Warehouse.Domain.Entities.Warehouse
        {
            ErpWarehouseCode = 110,
            WarehouseName = "B3",
            BranchCode = 1
        };
        var warehouseWithoutGram = new aqua_api.Modules.Warehouse.Domain.Entities.Warehouse
        {
            ErpWarehouseCode = 120,
            WarehouseName = "B4",
            BranchCode = 1
        };
        db.AddRange(stock, cageWithGram, cageWithoutGram, warehouseWithGram, warehouseWithoutGram);
        await db.SaveChangesAsync();

        db.CageWarehouseMappings.AddRange(
            new CageWarehouseMapping
            {
                CageId = cageWithGram.Id,
                WarehouseId = warehouseWithGram.Id,
                IsActive = true
            },
            new CageWarehouseMapping
            {
                CageId = cageWithoutGram.Id,
                WarehouseId = warehouseWithoutGram.Id,
                IsActive = true
            });
        await db.SaveChangesAsync();

        var receiptWithGram = new MalKabulVeSevkiyatDto
        {
            Tarih = new DateTime(2026, 1, 1),
            FisNo = "ERP-GR-WITH-GRAM",
            KafesKodu = 110,
            ProjeKodu = "ERP-PRJ-WITH-GRAM",
            StokKodu = stock.ErpStockCode,
            StokAdi = stock.StockName,
            Miktar = 100,
            BirimGram = 12.5m,
            HareketTuru = "J",
            GcKodu = "G",
            GrupKodu = "BALIK",
            IslemTuru = "Mal Kabul (Balik Girisi)"
        };
        var receiptWithoutGram = new MalKabulVeSevkiyatDto
        {
            Tarih = new DateTime(2026, 1, 1),
            FisNo = "ERP-GR-WITHOUT-GRAM",
            KafesKodu = 120,
            ProjeKodu = "ERP-PRJ-WITHOUT-GRAM",
            StokKodu = stock.ErpStockCode,
            StokAdi = stock.StockName,
            Miktar = 100,
            BirimGram = null,
            HareketTuru = "J",
            GcKodu = "G",
            GrupKodu = "BALIK",
            IslemTuru = "Mal Kabul (Balik Girisi)"
        };

        await job.ProcessMovementInCurrentTransactionAsync(receiptWithGram, "ERP-OPTIONAL-GRAM:1");
        await job.ProcessMovementInCurrentTransactionAsync(receiptWithoutGram, "ERP-OPTIONAL-GRAM:2");

        var lineWithGram = await db.GoodsReceiptLines.SingleAsync(x => x.ErpSourceMovementKey == "ERP-OPTIONAL-GRAM:1");
        var lineWithoutGram = await db.GoodsReceiptLines.SingleAsync(x => x.ErpSourceMovementKey == "ERP-OPTIONAL-GRAM:2");
        var batchWithGram = await db.FishBatches.SingleAsync(x => x.Project!.ProjectCode == "ERP-PRJ-WITH-GRAM");
        var batchWithoutGram = await db.FishBatches.SingleAsync(x => x.Project!.ProjectCode == "ERP-PRJ-WITHOUT-GRAM");

        Assert.Equal(12.5m, lineWithGram.FishAverageGram);
        Assert.Equal(1_250m, lineWithGram.FishTotalGram);
        Assert.Equal(12.5m, batchWithGram.CurrentAverageGram);
        Assert.Equal(1m, lineWithoutGram.FishAverageGram);
        Assert.Equal(100m, lineWithoutGram.FishTotalGram);
        Assert.Equal(1m, batchWithoutGram.CurrentAverageGram);

        var shipmentAfterFallbackOpening = new MalKabulVeSevkiyatDto
        {
            Tarih = new DateTime(2026, 1, 2),
            FisNo = "ERP-SH-AFTER-FALLBACK",
            KafesKodu = 120,
            ProjeKodu = "ERP-PRJ-WITHOUT-GRAM",
            StokKodu = stock.ErpStockCode,
            StokAdi = stock.StockName,
            Miktar = 40,
            HareketTuru = "J",
            GcKodu = "C",
            GrupKodu = "BALIK",
            IslemTuru = "Sevkiyat"
        };
        await job.ProcessMovementInCurrentTransactionAsync(
            shipmentAfterFallbackOpening,
            "ERP-OPTIONAL-GRAM-SHIPMENT:2");

        receiptWithoutGram.BirimGram = 14m;
        await job.ProcessMovementInCurrentTransactionAsync(receiptWithoutGram, "ERP-OPTIONAL-GRAM:2");

        lineWithoutGram = await db.GoodsReceiptLines.SingleAsync(x => x.ErpSourceMovementKey == "ERP-OPTIONAL-GRAM:2");
        var correctedShipment = await db.ShipmentLines.SingleAsync(x =>
            x.ErpSourceMovementKey == "ERP-OPTIONAL-GRAM-SHIPMENT:2");
        var correctedBalance = await db.BatchCageBalances.SingleAsync(x => x.FishBatchId == batchWithoutGram.Id);
        Assert.Equal(14m, lineWithoutGram.FishAverageGram);
        Assert.Equal(1_400m, lineWithoutGram.FishTotalGram);
        Assert.Equal(14m, correctedShipment.AverageGram);
        Assert.Equal(560m, correctedShipment.BiomassGram);
        Assert.Equal(60, correctedBalance.LiveCount);
        Assert.Equal(14m, correctedBalance.AverageGram);
        Assert.Equal(840m, correctedBalance.BiomassGram);
        Assert.Equal(14m, batchWithoutGram.CurrentAverageGram);
        Assert.Equal(1, await db.GoodsReceiptLines.CountAsync(x => x.ErpSourceMovementKey == "ERP-OPTIONAL-GRAM:2"));

        var projectWithoutGram = await db.Projects.SingleAsync(x => x.ProjectCode == "ERP-PRJ-WITHOUT-GRAM");
        var reportResponse = await reportService.GetReportAsync(new DevirFcrReportRequestDto
        {
            ProjectIds = new List<long> { projectWithoutGram.Id },
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 1, 2)
        });
        Assert.True(reportResponse.Success, reportResponse.ExceptionMessage);
        var report = Assert.Single(reportResponse.Data!.Rows);
        Assert.Equal(100, report.OpeningFishCount);
        Assert.Equal(40, report.ShipmentFishCount);
        Assert.Equal(60, report.EndingFishCount);
        Assert.Equal(14m, report.EndingAverageGram);
        Assert.Equal(1.4m, report.OpeningBiomassKg);
        Assert.Equal(0.56m, report.ShippedBiomassKg);
        Assert.Equal(0.84m, report.EndingBiomassKg);

        receiptWithoutGram.Miktar = 120;
        receiptWithoutGram.BirimGram = 16m;
        await job.ProcessMovementInCurrentTransactionAsync(receiptWithoutGram, "ERP-OPTIONAL-GRAM:2");
        lineWithoutGram = await db.GoodsReceiptLines.SingleAsync(x => x.ErpSourceMovementKey == "ERP-OPTIONAL-GRAM:2");
        correctedShipment = await db.ShipmentLines.SingleAsync(x =>
            x.ErpSourceMovementKey == "ERP-OPTIONAL-GRAM-SHIPMENT:2");
        correctedBalance = await db.BatchCageBalances.SingleAsync(x => x.FishBatchId == batchWithoutGram.Id);
        var mixedCorrection = await db.BatchMovements
            .Where(x => x.FishBatchId == batchWithoutGram.Id && x.MovementType == BatchMovementType.Stocking)
            .OrderByDescending(x => x.Id)
            .FirstAsync();
        Assert.Equal(120, lineWithoutGram.FishCount);
        Assert.Equal(16m, lineWithoutGram.FishAverageGram);
        Assert.Equal(1_920m, lineWithoutGram.FishTotalGram);
        Assert.Equal(40, correctedShipment.FishCount);
        Assert.Equal(16m, correctedShipment.AverageGram);
        Assert.Equal(640m, correctedShipment.BiomassGram);
        Assert.Equal(80, correctedBalance.LiveCount);
        Assert.Equal(16m, correctedBalance.AverageGram);
        Assert.Equal(1_280m, correctedBalance.BiomassGram);
        Assert.Equal(16m, batchWithoutGram.CurrentAverageGram);
        Assert.Equal(20, mixedCorrection.SignedCount);
        Assert.Equal(520m, mixedCorrection.SignedBiomassGram);
        Assert.Equal(14m, mixedCorrection.FromAverageGram);
        Assert.Equal(16m, mixedCorrection.ToAverageGram);

        reportResponse = await reportService.GetReportAsync(new DevirFcrReportRequestDto
        {
            ProjectIds = new List<long> { projectWithoutGram.Id },
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 1, 2)
        });
        Assert.True(reportResponse.Success, reportResponse.ExceptionMessage);
        report = Assert.Single(reportResponse.Data!.Rows);
        Assert.Equal(120, report.OpeningFishCount);
        Assert.Equal(40, report.ShipmentFishCount);
        Assert.Equal(80, report.EndingFishCount);
        Assert.Equal(16m, report.EndingAverageGram);
        Assert.Equal(1.92m, report.OpeningBiomassKg);
        Assert.Equal(0.64m, report.ShippedBiomassKg);
        Assert.Equal(1.28m, report.EndingBiomassKg);

        await job.ProcessMovementInCurrentTransactionAsync(receiptWithoutGram, "ERP-OPTIONAL-GRAM:2");
        Assert.Equal(4, await db.BatchMovements.CountAsync(x => x.FishBatchId == batchWithoutGram.Id));

        var overShipment = new MalKabulVeSevkiyatDto
        {
            Tarih = new DateTime(2026, 1, 3),
            FisNo = "ERP-SH-OVER-BALANCE",
            KafesKodu = 120,
            ProjeKodu = projectWithoutGram.ProjectCode,
            StokKodu = stock.ErpStockCode,
            StokAdi = stock.StockName,
            Miktar = 81,
            HareketTuru = "J",
            GcKodu = "C",
            GrupKodu = "BALIK",
            IslemTuru = "Sevkiyat"
        };
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            job.ProcessMovementInCurrentTransactionAsync(overShipment, "ERP-OVER-SHIPMENT:1"));

        Assert.False(await db.ShipmentLines.AnyAsync(x => x.ErpSourceMovementKey == "ERP-OVER-SHIPMENT:1"));
        Assert.False(await db.ErpReceiptShipmentMovements.AnyAsync(x => x.SourceMovementKey == "ERP-OVER-SHIPMENT:1"));
        var balanceAfterRollback = await db.BatchCageBalances.SingleAsync(x => x.FishBatchId == batchWithoutGram.Id);
        Assert.Equal(80, balanceAfterRollback.LiveCount);
        Assert.Equal(1_280m, balanceAfterRollback.BiomassGram);

        unitOfWork.Dispose();
    }

    [Fact]
    public async Task AutomaticSync_RepairsLateErpGram_AndStaleMirrorLink_Idempotently()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AquaDbContext>().UseSqlite(connection).Options;
        await using var db = new SyncJobSqliteAquaDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var stock = new Stock
        {
            ErpStockCode = "L-AUTO-GRAM",
            StockName = "Levrek Auto Gram",
            Unit = "ADET",
            GrupKodu = "BALIK",
            BranchCode = 1
        };
        var cage = new Cage { CageCode = "AUTO", CageName = "Auto Cage" };
        var warehouse = new aqua_api.Modules.Warehouse.Domain.Entities.Warehouse
        {
            ErpWarehouseCode = 151,
            WarehouseName = "Auto Cage",
            BranchCode = 1
        };
        db.AddRange(stock, cage, warehouse);
        await db.SaveChangesAsync();
        db.CageWarehouseMappings.Add(new CageWarehouseMapping
        {
            CageId = cage.Id,
            WarehouseId = warehouse.Id,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var movement = new MalKabulVeSevkiyatDto
        {
            Tarih = new DateTime(2026, 1, 1),
            FisNo = "ERP-AUTO-GRAM",
            KafesKodu = 151,
            ProjeKodu = "ERP-AUTO-PROJECT",
            StokKodu = stock.ErpStockCode,
            StokAdi = stock.StockName,
            Miktar = 100,
            HareketTuru = "J",
            GcKodu = "G",
            GrupKodu = "BALIK",
            IslemTuru = "Mal Kabul (Balik Girisi)"
        };
        var rows = new List<MalKabulVeSevkiyatDto> { movement };
        var unitOfWork = new EfUnitOfWork(db, new HttpContextAccessor());
        var localization = new LocalizationService(NullLogger<LocalizationService>.Instance);
        var job = new ErpReceiptShipmentMovementSyncJob(
            new FakeNetsisReadService(rows),
            db,
            unitOfWork,
            new BalanceLedgerManager(unitOfWork, localization),
            new aqua_api.Modules.FishGrowths.Application.Services.FishGrowthLedgerReplayService(unitOfWork, localization),
            localization,
            NullLogger<ErpReceiptShipmentMovementSyncJob>.Instance);

        await job.ExecuteAsync();
        var receiptLine = await db.GoodsReceiptLines.SingleAsync();
        var mirror = await db.ErpReceiptShipmentMovements.SingleAsync();
        Assert.Equal(1m, receiptLine.FishAverageGram);
        Assert.Equal(1, mirror.ProcessingAttemptCount);

        movement.BirimGram = 14m;
        await job.ExecuteAsync();
        receiptLine = await db.GoodsReceiptLines.SingleAsync();
        mirror = await db.ErpReceiptShipmentMovements.SingleAsync();
        Assert.Equal(14m, receiptLine.FishAverageGram);
        Assert.Equal(1_400m, receiptLine.FishTotalGram);
        Assert.Equal(2, mirror.ProcessingAttemptCount);
        Assert.Equal(1, await db.GoodsReceiptLines.CountAsync());

        await job.ExecuteAsync();
        mirror = await db.ErpReceiptShipmentMovements.SingleAsync();
        Assert.Equal(2, mirror.ProcessingAttemptCount);
        Assert.Equal(2, await db.BatchMovements.CountAsync());

        mirror.GoodsReceiptLineId = null;
        await db.SaveChangesAsync();
        await job.ExecuteAsync();
        mirror = await db.ErpReceiptShipmentMovements.SingleAsync();
        Assert.Equal(receiptLine.Id, mirror.GoodsReceiptLineId);
        Assert.Equal(3, mirror.ProcessingAttemptCount);
        Assert.Equal(1, await db.GoodsReceiptLines.CountAsync());
        Assert.Equal(2, await db.BatchMovements.CountAsync());

        unitOfWork.Dispose();
    }

    [Fact]
    public async Task MultiCageReceiptCorrection_UsesLocationGram_AndKeepsWeightedBatchAverage()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AquaDbContext>().UseSqlite(connection).Options;
        await using var db = new SyncJobSqliteAquaDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var unitOfWork = new EfUnitOfWork(db, new HttpContextAccessor());
        var localization = new LocalizationService(NullLogger<LocalizationService>.Instance);
        var ledger = new BalanceLedgerManager(unitOfWork, localization);
        var replay = new aqua_api.Modules.FishGrowths.Application.Services.FishGrowthLedgerReplayService(unitOfWork, localization);
        var job = new ErpReceiptShipmentMovementSyncJob(
            null!, db, unitOfWork, ledger, replay, localization,
            NullLogger<ErpReceiptShipmentMovementSyncJob>.Instance);
        var reportService = new DevirFcrReportService(unitOfWork, localization);

        var openingDate = new DateTime(2026, 1, 1);
        var project = new Project
        {
            ProjectCode = "ERP-MULTI-CAGE",
            ProjectName = "ERP Multi Cage",
            StartDate = openingDate,
            Status = DocumentStatus.Posted
        };
        var stock = new Stock
        {
            ErpStockCode = "L-MULTI",
            StockName = "Levrek Multi",
            Unit = "ADET",
            GrupKodu = "BALIK",
            BranchCode = 1
        };
        var cageA = new Cage { CageCode = "MA", CageName = "Multi A" };
        var cageB = new Cage { CageCode = "MB", CageName = "Multi B" };
        var warehouseA = new aqua_api.Modules.Warehouse.Domain.Entities.Warehouse
        {
            ErpWarehouseCode = 141,
            WarehouseName = "Multi A",
            BranchCode = 1
        };
        var warehouseB = new aqua_api.Modules.Warehouse.Domain.Entities.Warehouse
        {
            ErpWarehouseCode = 142,
            WarehouseName = "Multi B",
            BranchCode = 1
        };
        db.AddRange(project, stock, cageA, cageB, warehouseA, warehouseB);
        await db.SaveChangesAsync();

        db.CageWarehouseMappings.AddRange(
            new CageWarehouseMapping { CageId = cageA.Id, WarehouseId = warehouseA.Id, IsActive = true },
            new CageWarehouseMapping { CageId = cageB.Id, WarehouseId = warehouseB.Id, IsActive = true });
        var projectCageA = new ProjectCage { ProjectId = project.Id, CageId = cageA.Id, AssignedDate = openingDate };
        var projectCageB = new ProjectCage { ProjectId = project.Id, CageId = cageB.Id, AssignedDate = openingDate };
        db.ProjectCages.AddRange(projectCageA, projectCageB);
        await db.SaveChangesAsync();

        var batch = new FishBatch
        {
            ProjectId = project.Id,
            BatchCode = "BATCH-MULTI",
            FishStockId = stock.Id,
            CurrentAverageGram = 250m,
            StartDate = openingDate
        };
        db.FishBatches.Add(batch);
        await db.SaveChangesAsync();
        await ledger.ApplyDelta(
            project.Id, batch.Id, projectCageA.Id, 100, 10_000m,
            BatchMovementType.OpeningImport, openingDate, "Opening A", "TEST_OPENING", 141,
            null, projectCageA.Id, null, stock.Id, null, 100m);
        await ledger.ApplyDelta(
            project.Id, batch.Id, projectCageB.Id, 300, 90_000m,
            BatchMovementType.OpeningImport, openingDate, "Opening B", "TEST_OPENING", 142,
            null, projectCageB.Id, null, stock.Id, null, 300m);
        await db.SaveChangesAsync();

        var receipt = new MalKabulVeSevkiyatDto
        {
            Tarih = openingDate.AddDays(1),
            FisNo = "ERP-MULTI-GR",
            KafesKodu = 141,
            ProjeKodu = project.ProjectCode,
            StokKodu = stock.ErpStockCode,
            StokAdi = stock.StockName,
            Miktar = 100,
            HareketTuru = "J",
            GcKodu = "G",
            GrupKodu = "BALIK",
            IslemTuru = "Mal Kabul (Balik Girisi)"
        };
        var shipment = new MalKabulVeSevkiyatDto
        {
            Tarih = openingDate.AddDays(2),
            FisNo = "ERP-MULTI-SH",
            KafesKodu = 141,
            ProjeKodu = project.ProjectCode,
            StokKodu = stock.ErpStockCode,
            StokAdi = stock.StockName,
            Miktar = 50,
            HareketTuru = "J",
            GcKodu = "C",
            GrupKodu = "BALIK",
            IslemTuru = "Sevkiyat"
        };

        await job.ProcessMovementInCurrentTransactionAsync(receipt, "ERP-MULTI-GR:1");
        await job.ProcessMovementInCurrentTransactionAsync(shipment, "ERP-MULTI-SH:1");

        var line = await db.GoodsReceiptLines.SingleAsync(x => x.ErpSourceMovementKey == "ERP-MULTI-GR:1");
        var shipmentLine = await db.ShipmentLines.SingleAsync(x => x.ErpSourceMovementKey == "ERP-MULTI-SH:1");
        Assert.Equal(100m, line.FishAverageGram);
        Assert.Equal(100m, shipmentLine.AverageGram);
        Assert.Equal(233.333m, batch.CurrentAverageGram);

        receipt.BirimGram = 120m;
        await job.ProcessMovementInCurrentTransactionAsync(receipt, "ERP-MULTI-GR:1");

        line = await db.GoodsReceiptLines.SingleAsync(x => x.ErpSourceMovementKey == "ERP-MULTI-GR:1");
        shipmentLine = await db.ShipmentLines.SingleAsync(x => x.ErpSourceMovementKey == "ERP-MULTI-SH:1");
        var balanceA = await db.BatchCageBalances.SingleAsync(x => x.FishBatchId == batch.Id && x.ProjectCageId == projectCageA.Id);
        var balanceB = await db.BatchCageBalances.SingleAsync(x => x.FishBatchId == batch.Id && x.ProjectCageId == projectCageB.Id);
        Assert.Equal(120m, line.FishAverageGram);
        Assert.Equal(110m, shipmentLine.AverageGram);
        Assert.Equal(150, balanceA.LiveCount);
        Assert.Equal(16_500m, balanceA.BiomassGram);
        Assert.Equal(300, balanceB.LiveCount);
        Assert.Equal(90_000m, balanceB.BiomassGram);
        Assert.Equal(236.667m, batch.CurrentAverageGram);

        var reportResponse = await reportService.GetReportAsync(new DevirFcrReportRequestDto
        {
            ProjectIds = new List<long> { project.Id },
            FromDate = openingDate,
            ToDate = openingDate.AddDays(2)
        });
        Assert.True(reportResponse.Success, reportResponse.ExceptionMessage);
        var report = Assert.Single(reportResponse.Data!.Rows);
        Assert.Equal(400, report.OpeningFishCount);
        Assert.Equal(50, report.ShipmentFishCount);
        Assert.Equal(450, report.EndingFishCount);
        Assert.Equal(236.667m, report.EndingAverageGram);
        Assert.Equal(100m, report.OpeningBiomassKg);
        Assert.Equal(5.5m, report.ShippedBiomassKg);
        Assert.Equal(106.5m, report.EndingBiomassKg);

        unitOfWork.Dispose();
    }

    [Fact]
    public async Task CorrectedErpOpeningGram_ReplaysGrowthMortalityShipmentAndReport_Atomically()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AquaDbContext>().UseSqlite(connection).Options;
        await using var db = new SyncJobSqliteAquaDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var unitOfWork = new EfUnitOfWork(db, new HttpContextAccessor());
        var localization = new LocalizationService(NullLogger<LocalizationService>.Instance);
        var ledger = new BalanceLedgerManager(unitOfWork, localization);
        var replay = new aqua_api.Modules.FishGrowths.Application.Services.FishGrowthLedgerReplayService(unitOfWork, localization);
        var job = new ErpReceiptShipmentMovementSyncJob(
            null!,
            db,
            unitOfWork,
            ledger,
            replay,
            localization,
            NullLogger<ErpReceiptShipmentMovementSyncJob>.Instance);
        var reportService = new DevirFcrReportService(unitOfWork, localization);

        var stock = new Stock
        {
            ErpStockCode = "L-COMPLEX",
            StockName = "Levrek Complex",
            Unit = "ADET",
            GrupKodu = "BALIK",
            BranchCode = 1
        };
        var feedStock = new Stock
        {
            ErpStockCode = "Y-COMPLEX",
            StockName = "Test Yemi",
            Unit = "KG",
            GrupKodu = "YEM",
            BranchCode = 1
        };
        var cage = new Cage { CageCode = "CX", CageName = "Complex Cage" };
        var warehouse = new aqua_api.Modules.Warehouse.Domain.Entities.Warehouse
        {
            ErpWarehouseCode = 130,
            WarehouseName = "CX",
            BranchCode = 1
        };
        db.AddRange(stock, feedStock, cage, warehouse);
        await db.SaveChangesAsync();
        db.CageWarehouseMappings.Add(new CageWarehouseMapping
        {
            CageId = cage.Id,
            WarehouseId = warehouse.Id,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var receiptMovement = new MalKabulVeSevkiyatDto
        {
            Tarih = new DateTime(2026, 1, 1),
            FisNo = "ERP-GR-COMPLEX",
            KafesKodu = 130,
            ProjeKodu = "ERP-PRJ-COMPLEX",
            StokKodu = stock.ErpStockCode,
            StokAdi = stock.StockName,
            Miktar = 1_000,
            BirimGram = null,
            HareketTuru = "J",
            GcKodu = "G",
            GrupKodu = "BALIK",
            IslemTuru = "Mal Kabul (Balik Girisi)"
        };
        await job.ProcessMovementInCurrentTransactionAsync(receiptMovement, "ERP-COMPLEX-RECEIPT:1");

        var project = await db.Projects.SingleAsync(x => x.ProjectCode == "ERP-PRJ-COMPLEX");
        var projectCage = await db.ProjectCages.SingleAsync(x => x.ProjectId == project.Id);
        var batch = await db.FishBatches.SingleAsync(x => x.ProjectId == project.Id);

        var growth = new aqua_api.Modules.FishGrowths.Domain.Entities.FishGrowth
        {
            ProjectId = project.Id,
            ProjectCageId = projectCage.Id,
            FishBatchId = batch.Id,
            GrowthDate = new DateTime(2026, 1, 5),
            GrowthYear = 2026,
            GrowthMonth = 1,
            FishCount = 1_000,
            PreviousAverageGram = 1m,
            GrowthGram = 19m,
            NewAverageGram = 20m,
            PreviousBiomassGram = 1_000m,
            NewBiomassGram = 20_000m
        };
        db.FishGrowths.Add(growth);
        await db.SaveChangesAsync();
        await ledger.ApplyDelta(
            project.Id,
            batch.Id,
            projectCage.Id,
            0,
            19_000m,
            BatchMovementType.FishGrowth,
            growth.GrowthDate,
            "Complex growth",
            "RII_FISH_GROWTH",
            growth.Id,
            projectCage.Id,
            projectCage.Id,
            stock.Id,
            stock.Id,
            1m,
            20m);
        batch.CurrentAverageGram = 20m;
        await db.SaveChangesAsync();

        var mortality = new Mortality
        {
            ProjectId = project.Id,
            MortalityNo = "MORT-COMPLEX",
            MortalityDate = new DateTime(2026, 1, 10),
            Status = DocumentStatus.Posted
        };
        var feeding = new Feeding
        {
            ProjectId = project.Id,
            FeedingNo = "FEED-COMPLEX",
            FeedingDate = new DateTime(2026, 1, 12),
            FeedingSlot = FeedingSlot.Morning,
            SourceType = FeedingSourceType.Manual,
            Status = DocumentStatus.Posted
        };
        db.AddRange(mortality, feeding);
        await db.SaveChangesAsync();

        var mortalityLine = new MortalityLine
        {
            MortalityId = mortality.Id,
            FishBatchId = batch.Id,
            ProjectCageId = projectCage.Id,
            DeadCount = 100
        };
        var feedingLine = new FeedingLine
        {
            FeedingId = feeding.Id,
            StockId = feedStock.Id,
            QtyUnit = 5m,
            GramPerUnit = 1_000m,
            TotalGram = 5_000m
        };
        db.AddRange(mortalityLine, feedingLine);
        await db.SaveChangesAsync();
        var feedingDistribution = new FeedingDistribution
        {
            FeedingLineId = feedingLine.Id,
            FishBatchId = batch.Id,
            ProjectCageId = projectCage.Id,
            FeedGram = 5_000m
        };
        db.FeedingDistributions.Add(feedingDistribution);
        await db.SaveChangesAsync();

        await ledger.ApplyDelta(
            project.Id,
            batch.Id,
            projectCage.Id,
            -100,
            -2_000m,
            BatchMovementType.Mortality,
            mortality.MortalityDate,
            "Complex mortality",
            "RII_MORTALITY",
            mortality.Id,
            projectCage.Id,
            null,
            stock.Id,
            null,
            20m,
            null);
        db.BatchMovements.Add(new BatchMovement
        {
            FishBatchId = batch.Id,
            ProjectCageId = projectCage.Id,
            MovementDate = feeding.FeedingDate,
            MovementType = BatchMovementType.Feeding,
            FeedGram = 5_000m,
            ReferenceTable = "RII_FEEDING_DISTRIBUTION",
            ReferenceId = feedingDistribution.Id
        });
        await db.SaveChangesAsync();

        var shipmentMovement = new MalKabulVeSevkiyatDto
        {
            Tarih = new DateTime(2026, 1, 15),
            FisNo = "ERP-SH-COMPLEX",
            KafesKodu = 130,
            ProjeKodu = project.ProjectCode,
            StokKodu = stock.ErpStockCode,
            StokAdi = stock.StockName,
            Miktar = 200,
            HareketTuru = "J",
            GcKodu = "C",
            GrupKodu = "BALIK",
            IslemTuru = "Sevkiyat"
        };
        await job.ProcessMovementInCurrentTransactionAsync(shipmentMovement, "ERP-COMPLEX-SHIPMENT:1");

        var movementCountBeforeGramlessResync = await db.BatchMovements.CountAsync(x => x.FishBatchId == batch.Id);
        await job.ProcessMovementInCurrentTransactionAsync(receiptMovement, "ERP-COMPLEX-RECEIPT:1");
        var gramlessResyncedReceipt = await db.GoodsReceiptLines.SingleAsync(x =>
            x.ErpSourceMovementKey == "ERP-COMPLEX-RECEIPT:1");
        Assert.Equal(1m, gramlessResyncedReceipt.FishAverageGram);
        Assert.Equal(movementCountBeforeGramlessResync, await db.BatchMovements.CountAsync(x => x.FishBatchId == batch.Id));

        receiptMovement.BirimGram = 10m;
        await job.ProcessMovementInCurrentTransactionAsync(receiptMovement, "ERP-COMPLEX-RECEIPT:1");

        var correctedGrowth = await db.FishGrowths.SingleAsync(x => x.Id == growth.Id);
        var correctedMortalityMovement = await db.BatchMovements.SingleAsync(x =>
            x.ReferenceTable == "RII_MORTALITY" && x.ReferenceId == mortality.Id);
        var correctedShipment = await db.ShipmentLines.SingleAsync(x =>
            x.ErpSourceMovementKey == "ERP-COMPLEX-SHIPMENT:1");
        var correctedBalance = await db.BatchCageBalances.SingleAsync(x =>
            x.FishBatchId == batch.Id && x.ProjectCageId == projectCage.Id);

        Assert.Equal(10m, correctedGrowth.PreviousAverageGram);
        Assert.Equal(10m, correctedGrowth.GrowthGram);
        Assert.Equal(20m, correctedGrowth.NewAverageGram);
        Assert.Equal(-2_000m, correctedMortalityMovement.SignedBiomassGram);
        Assert.Equal(20m, correctedShipment.AverageGram);
        Assert.Equal(4_000m, correctedShipment.BiomassGram);
        Assert.Equal(700, correctedBalance.LiveCount);
        Assert.Equal(20m, correctedBalance.AverageGram);
        Assert.Equal(14_000m, correctedBalance.BiomassGram);

        var reportResponse = await reportService.GetReportAsync(new DevirFcrReportRequestDto
        {
            ProjectIds = new List<long> { project.Id },
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 1, 15)
        });
        Assert.True(reportResponse.Success, reportResponse.ExceptionMessage);
        var report = Assert.Single(reportResponse.Data!.Rows);
        Assert.Equal(1_000, report.OpeningFishCount);
        Assert.Equal(100, report.MortalityFishCount);
        Assert.Equal(10m, report.MortalityPct);
        Assert.Equal(200, report.ShipmentFishCount);
        Assert.Equal(700, report.EndingFishCount);
        Assert.Equal(10m, report.OpeningBiomassKg);
        Assert.Equal(14m, report.EndingBiomassKg);
        Assert.Equal(4m, report.ShippedBiomassKg);
        Assert.Equal(1m, report.MortalityBiomassKg);
        Assert.Equal(5m, report.TotalFeedKg);
        Assert.Equal(19m, report.ProducedBiomassKg);
        Assert.Equal(0.263m, report.Fcr);

        var laterGrowth = new aqua_api.Modules.FishGrowths.Domain.Entities.FishGrowth
        {
            ProjectId = project.Id,
            ProjectCageId = projectCage.Id,
            FishBatchId = batch.Id,
            GrowthDate = new DateTime(2026, 2, 5),
            GrowthYear = 2026,
            GrowthMonth = 2,
            FishCount = 700,
            PreviousAverageGram = 20m,
            GrowthGram = 10m,
            NewAverageGram = 30m,
            PreviousBiomassGram = 14_000m,
            NewBiomassGram = 21_000m
        };
        db.FishGrowths.Add(laterGrowth);
        await db.SaveChangesAsync();
        await ledger.ApplyDelta(
            project.Id,
            batch.Id,
            projectCage.Id,
            0,
            7_000m,
            BatchMovementType.FishGrowth,
            laterGrowth.GrowthDate,
            "Later complex growth",
            "RII_FISH_GROWTH",
            laterGrowth.Id,
            projectCage.Id,
            projectCage.Id,
            stock.Id,
            stock.Id,
            20m,
            30m);
        batch.CurrentAverageGram = 30m;
        await db.SaveChangesAsync();

        // Reprocessing an old shipment must use its historical 20 g state, not today's 30 g state.
        await job.ProcessMovementInCurrentTransactionAsync(shipmentMovement, "ERP-COMPLEX-SHIPMENT:1");
        correctedShipment = await db.ShipmentLines.SingleAsync(x =>
            x.ErpSourceMovementKey == "ERP-COMPLEX-SHIPMENT:1");
        correctedBalance = await db.BatchCageBalances.SingleAsync(x =>
            x.FishBatchId == batch.Id && x.ProjectCageId == projectCage.Id);
        Assert.Equal(20m, correctedShipment.AverageGram);
        Assert.Equal(4_000m, correctedShipment.BiomassGram);
        Assert.Equal(700, correctedBalance.LiveCount);
        Assert.Equal(30m, correctedBalance.AverageGram);
        Assert.Equal(21_000m, correctedBalance.BiomassGram);

        shipmentMovement.Miktar = 250;
        await job.ProcessMovementInCurrentTransactionAsync(shipmentMovement, "ERP-COMPLEX-SHIPMENT:1");
        correctedShipment = await db.ShipmentLines.SingleAsync(x =>
            x.ErpSourceMovementKey == "ERP-COMPLEX-SHIPMENT:1");
        correctedBalance = await db.BatchCageBalances.SingleAsync(x =>
            x.FishBatchId == batch.Id && x.ProjectCageId == projectCage.Id);
        var replayedLaterGrowth = await db.FishGrowths.SingleAsync(x => x.Id == laterGrowth.Id);
        Assert.Equal(250, correctedShipment.FishCount);
        Assert.Equal(20m, correctedShipment.AverageGram);
        Assert.Equal(5_000m, correctedShipment.BiomassGram);
        Assert.Equal(650, correctedBalance.LiveCount);
        Assert.Equal(30m, correctedBalance.AverageGram);
        Assert.Equal(19_500m, correctedBalance.BiomassGram);
        Assert.Equal(650, replayedLaterGrowth.FishCount);
        Assert.Equal(13_000m, replayedLaterGrowth.PreviousBiomassGram);
        Assert.Equal(19_500m, replayedLaterGrowth.NewBiomassGram);

        receiptMovement.BirimGram = 25m;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            job.ProcessMovementInCurrentTransactionAsync(receiptMovement, "ERP-COMPLEX-RECEIPT:1"));

        var receiptAfterRollback = await db.GoodsReceiptLines.SingleAsync(x =>
            x.ErpSourceMovementKey == "ERP-COMPLEX-RECEIPT:1");
        var growthAfterRollback = await db.FishGrowths.SingleAsync(x => x.Id == growth.Id);
        var shipmentAfterRollback = await db.ShipmentLines.SingleAsync(x =>
            x.ErpSourceMovementKey == "ERP-COMPLEX-SHIPMENT:1");
        var balanceAfterRollback = await db.BatchCageBalances.SingleAsync(x =>
            x.FishBatchId == batch.Id && x.ProjectCageId == projectCage.Id);
        Assert.Equal(10m, receiptAfterRollback.FishAverageGram);
        Assert.Equal(10m, growthAfterRollback.PreviousAverageGram);
        Assert.Equal(20m, shipmentAfterRollback.AverageGram);
        Assert.Equal(250, shipmentAfterRollback.FishCount);
        Assert.Equal(650, balanceAfterRollback.LiveCount);
        Assert.Equal(19_500m, balanceAfterRollback.BiomassGram);

        db.BatchMovements.Add(new BatchMovement
        {
            FishBatchId = batch.Id,
            ProjectCageId = projectCage.Id,
            MovementDate = new DateTime(2026, 3, 1),
            MovementType = BatchMovementType.Transfer,
            SignedCount = 0,
            SignedBiomassGram = 0m,
            ReferenceTable = "TEST_STRUCTURAL_TRANSFER",
            ReferenceId = 1
        });
        await db.SaveChangesAsync();

        receiptMovement.BirimGram = 11m;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            job.ProcessMovementInCurrentTransactionAsync(receiptMovement, "ERP-COMPLEX-RECEIPT:1"));
        receiptAfterRollback = await db.GoodsReceiptLines.SingleAsync(x =>
            x.ErpSourceMovementKey == "ERP-COMPLEX-RECEIPT:1");
        balanceAfterRollback = await db.BatchCageBalances.SingleAsync(x =>
            x.FishBatchId == batch.Id && x.ProjectCageId == projectCage.Id);
        Assert.Equal(10m, receiptAfterRollback.FishAverageGram);
        Assert.Equal(650, balanceAfterRollback.LiveCount);
        Assert.Equal(19_500m, balanceAfterRollback.BiomassGram);

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

            var feedingEntity = modelBuilder.Model.FindEntityType(typeof(Feeding));
            var feedingDateOnly = feedingEntity?.FindProperty("FeedingDateOnly");
            feedingDateOnly?.SetAnnotation("Relational:ComputedColumnSql", "date(FeedingDate)");
            feedingDateOnly?.SetAnnotation("Relational:IsStored", true);
        }
    }

    private sealed class FakeNetsisReadService(List<MalKabulVeSevkiyatDto> rows) : INetsisReadService
    {
        public Task<ApiResponse<List<MalKabulVeSevkiyatDto>>> GetGoodsReceiptAndShipmentMovementsAsync(DateTime? startDate = null) =>
            Task.FromResult(ApiResponse<List<MalKabulVeSevkiyatDto>>.SuccessResult(rows, "OK"));

        public Task<ApiResponse<short>> GetBranchCodeFromContextAsync() => throw new NotSupportedException();
        public Task<ApiResponse<List<CariDto>>> GetCustomersAsync(string? customerCode) => throw new NotSupportedException();
        public Task<ApiResponse<PagedResponse<CariDto>>> GetCustomersPagedAsync(int pageNumber, int pageSize, string? search, string? sortBy, string? sortDirection) => throw new NotSupportedException();
        public Task<ApiResponse<List<CariDto>>> GetCustomersByCodesAsync(IEnumerable<string> customerCodes) => throw new NotSupportedException();
        public Task<ApiResponse<List<DepoDto>>> GetWarehousesAsync(short? warehouseCode) => throw new NotSupportedException();
        public Task<ApiResponse<PagedResponse<DepoDto>>> GetWarehousesPagedAsync(int pageNumber, int pageSize, string? search, string? sortBy, string? sortDirection) => throw new NotSupportedException();
        public Task<ApiResponse<List<StokFunctionDto>>> GetStocksAsync(string? stockCode) => throw new NotSupportedException();
        public Task<ApiResponse<PagedResponse<StokFunctionDto>>> GetStocksPagedAsync(int pageNumber, int pageSize, string? search, string? sortBy, string? sortDirection) => throw new NotSupportedException();
        public Task<ApiResponse<List<BranchDto>>> GetBranchesAsync(int? branchNo = null) => throw new NotSupportedException();
        public Task<ApiResponse<PagedResponse<BranchDto>>> GetBranchesPagedAsync(int pageNumber, int pageSize, string? search, string? sortBy, string? sortDirection) => throw new NotSupportedException();
        public Task<ApiResponse<List<KurDto>>> GetExchangeRatesAsync(DateTime date, int pricingType) => throw new NotSupportedException();
        public Task<ApiResponse<List<ErpShippingAddressDto>>> GetShippingAddressesAsync(string customerCode) => throw new NotSupportedException();
        public Task<ApiResponse<List<StokGroupDto>>> GetStockGroupsAsync(string? groupCode) => throw new NotSupportedException();
        public Task<ApiResponse<List<ProjeDto>>> GetProjectsAsync() => throw new NotSupportedException();
        public Task<ApiResponse<PagedResponse<MalKabulVeSevkiyatDto>>> GetGoodsReceiptAndShipmentMovementsPagedAsync(int pageNumber, int pageSize, string? search, DateTime? startDate, string? sortBy, string? sortDirection) => throw new NotSupportedException();
        public Task<ApiResponse<List<ErpReceiptShipmentMovementDto>>> GetReceiptShipmentMovementMirrorAsync() => throw new NotSupportedException();
        public Task<ApiResponse<PagedResponse<ErpReceiptShipmentMovementDto>>> GetReceiptShipmentMovementMirrorPagedAsync(int pageNumber, int pageSize, string? search, string? sortBy, string? sortDirection) => throw new NotSupportedException();
        public Task<ApiResponse<object>> HealthCheckAsync() => throw new NotSupportedException();
    }
}
