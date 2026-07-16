using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.Integrations.Application.Dtos;
using aqua_api.Modules.Integrations.Application.Services;
using aqua_api.Modules.Integrations.Domain.Entities;
using aqua_api.Modules.Identity.Domain.Entities;
using aqua_api.Modules.Stock.Domain.Entities;
using aqua_api.Modules.System.Infrastructure.BackgroundJobs.Interfaces;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using aqua_api.Shared.Infrastructure.Services;

namespace aqua_api.Tests;

public sealed class ErpReceiptResyncServiceIntegrationTests
{
    private const string FishReceiptOperationType = "Mal Kabul (Balık Girişi)";

    [Fact]
    public async Task Preview_ListsDependentFeeding_AndAllowsSafeResync()
    {
        await using var fixture = await CreateFixtureAsync();
        var seeded = await SeedReceiptGraphAsync(fixture.Db, erpIntegratedFeeding: false);

        var response = await fixture.Service.PreviewAsync(seeded.DocumentNo, "G", FishReceiptOperationType);

        Assert.True(response.Success, response.ExceptionMessage);
        Assert.NotNull(response.Data);
        Assert.True(response.Data!.CanResync);
        Assert.False(response.Data.RequiresErpReversal);
        var impact = Assert.Single(response.Data.Impacts);
        Assert.Equal("Feeding", impact.OperationType);
        Assert.Equal(seeded.FishBatchId, impact.FishBatchId);
        Assert.Equal(2.5m, impact.FeedKg);
    }

    [Fact]
    public async Task Preview_BlocksResync_WhenDependentFeedingWasPostedToErp()
    {
        await using var fixture = await CreateFixtureAsync();
        var seeded = await SeedReceiptGraphAsync(fixture.Db, erpIntegratedFeeding: true);

        var response = await fixture.Service.PreviewAsync(seeded.DocumentNo, "G", FishReceiptOperationType);

        Assert.True(response.Success, response.ExceptionMessage);
        Assert.NotNull(response.Data);
        Assert.False(response.Data!.CanResync);
        Assert.True(response.Data.RequiresErpReversal);
        Assert.NotEmpty(response.Data.BlockingReasons);
        Assert.Contains(response.Data.Impacts, x => x.IsErpIntegrated && x.ErpReferenceNumber == "ERP-FEED-001");
    }

    [Fact]
    public async Task Resync_RollsBackEveryChange_WhenCurrentErpReprocessingFails()
    {
        var currentRows = new List<MalKabulVeSevkiyatDto>();
        await using var fixture = await CreateFixtureAsync(currentRows, throwDuringSync: true);
        var seeded = await SeedReceiptGraphAsync(fixture.Db, erpIntegratedFeeding: false, includeFeeding: false);
        currentRows.Add(new MalKabulVeSevkiyatDto
        {
            Tarih = seeded.ReceiptDate,
            FisNo = seeded.DocumentNo,
            KafesKodu = 110,
            ProjeKodu = "ERP-PRJ-001",
            StokKodu = "FISH-001",
            StokAdi = "Levrek",
            Miktar = 1_000,
            HareketTuru = "J",
            GcKodu = "G",
            GrupKodu = "BALIK",
            IslemTuru = "Mal Kabul (Balık Girişi)"
        });

        var response = await fixture.Service.ResyncAsync(new ErpReceiptResyncRequestDto
        {
            DocumentNo = seeded.DocumentNo,
            InOutCode = "G",
            OperationType = FishReceiptOperationType,
            ConfirmationDocumentNo = seeded.DocumentNo
        }, 1);

        Assert.False(response.Success);
        Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
        Assert.True(await fixture.Db.GoodsReceipts.AnyAsync(x => x.Id == seeded.GoodsReceiptId));
        Assert.True(await fixture.Db.FishBatches.AnyAsync(x => x.Id == seeded.FishBatchId));
        Assert.True(await fixture.Db.BatchCageBalances.AnyAsync(x => x.FishBatchId == seeded.FishBatchId && x.LiveCount == 1_000));
        Assert.Equal(1, await fixture.Db.BatchMovements.CountAsync(x => x.FishBatchId == seeded.FishBatchId));
        Assert.True(await fixture.Db.ErpReceiptShipmentMovements.AnyAsync(x => x.DocumentNo == seeded.DocumentNo && x.IsProcessed));
    }

    [Fact]
    public async Task Resync_SoftDeletesReceiptGraph_AndBalancesLedger_WhenReprocessingSucceeds()
    {
        var currentRows = new List<MalKabulVeSevkiyatDto>();
        await using var fixture = await CreateFixtureAsync(currentRows);
        var seeded = await SeedReceiptGraphAsync(fixture.Db, erpIntegratedFeeding: false);
        var feedStock = await fixture.Db.Stocks.SingleAsync(x => x.ErpStockCode == "FEED-001");
        var otherOperationLine = new GoodsReceiptLine
        {
            GoodsReceiptId = seeded.GoodsReceiptId,
            ItemType = GoodsReceiptItemType.Feed,
            StockId = feedStock.Id,
            QtyUnit = 250,
            GramPerUnit = 1_000,
            TotalGram = 250_000,
            ErpSourceMovementKey = "ERP-GR-001-FEED-LINE-1"
        };
        fixture.Db.GoodsReceiptLines.Add(otherOperationLine);
        await fixture.Db.SaveChangesAsync();
        fixture.Db.ErpReceiptShipmentMovements.Add(new ErpReceiptShipmentMovement
        {
            SourceSystem = "Netsis",
            SourceMovementKey = "ERP-GR-001-FEED-LINE-1",
            MovementDate = seeded.ReceiptDate,
            DocumentNo = seeded.DocumentNo,
            ErpWarehouseCode = 1,
            ErpProjectCode = "GENEL",
            ErpStockCode = "FEED-001",
            Quantity = 250,
            MovementKind = "J",
            InOutCode = "G",
            OperationType = "Mal Kabul (Yem Girişi)",
            GoodsReceiptId = seeded.GoodsReceiptId,
            GoodsReceiptLineId = otherOperationLine.Id,
            StockId = feedStock.Id,
            IsMatched = true,
            IsProcessed = true,
            LastSyncedAt = seeded.ReceiptDate,
            ProcessedAt = seeded.ReceiptDate
        });
        await fixture.Db.SaveChangesAsync();
        currentRows.Add(new MalKabulVeSevkiyatDto
        {
            Tarih = seeded.ReceiptDate,
            FisNo = seeded.DocumentNo,
            KafesKodu = 110,
            ProjeKodu = "ERP-PRJ-001",
            StokKodu = "FISH-001",
            StokAdi = "Levrek",
            Miktar = 1_000,
            HareketTuru = "J",
            GcKodu = "G",
            GrupKodu = "BALIK",
            IslemTuru = "Mal Kabul (Balık Girişi)"
        });

        var response = await fixture.Service.ResyncAsync(new ErpReceiptResyncRequestDto
        {
            DocumentNo = seeded.DocumentNo,
            InOutCode = "G",
            OperationType = FishReceiptOperationType,
            ConfirmationDocumentNo = seeded.DocumentNo
        }, 1);

        Assert.True(response.Success, response.ExceptionMessage);
        Assert.NotNull(response.Data);
        Assert.Equal(1, response.Data!.CancelledSourceMovementCount);
        Assert.Equal(2, response.Data.ReversedLedgerMovementCount);
        Assert.Equal(1, response.Data.ReprocessedSourceMovementCount);

        var oldReceipt = await fixture.Db.GoodsReceipts.IgnoreQueryFilters().SingleAsync(x => x.Id == seeded.GoodsReceiptId);
        var oldBatch = await fixture.Db.FishBatches.IgnoreQueryFilters().SingleAsync(x => x.Id == seeded.FishBatchId);
        var oldMirror = await fixture.Db.ErpReceiptShipmentMovements.IgnoreQueryFilters()
            .SingleAsync(x => x.DocumentNo == seeded.DocumentNo && x.OperationType == FishReceiptOperationType);
        var otherOperationMirror = await fixture.Db.ErpReceiptShipmentMovements
            .SingleAsync(x => x.DocumentNo == seeded.DocumentNo && x.OperationType == "Mal Kabul (Yem Girişi)");
        var oldFeeding = await fixture.Db.Feedings.IgnoreQueryFilters().SingleAsync(x => x.FeedingNo == "FD-001");
        var otherOperationReceiptLine = await fixture.Db.GoodsReceiptLines
            .SingleAsync(x => x.Id == otherOperationLine.Id);
        var selectedReceiptLine = await fixture.Db.GoodsReceiptLines.IgnoreQueryFilters()
            .SingleAsync(x => x.ErpSourceMovementKey == "ERP-GR-001-LINE-1");
        Assert.False(oldReceipt.IsDeleted);
        Assert.False(otherOperationReceiptLine.IsDeleted);
        Assert.True(selectedReceiptLine.IsDeleted);
        Assert.True(oldBatch.IsDeleted);
        Assert.True(oldMirror.IsDeleted);
        Assert.True(otherOperationMirror.IsProcessed);
        Assert.False(otherOperationMirror.IsDeleted);
        Assert.True(oldFeeding.IsDeleted);

        var ledger = await fixture.Db.BatchMovements.Where(x => x.FishBatchId == seeded.FishBatchId).ToListAsync();
        Assert.Equal(4, ledger.Count);
        Assert.Equal(0, ledger.Sum(x => x.SignedCount));
        Assert.Equal(0m, ledger.Sum(x => x.SignedBiomassGram));
        Assert.Equal(0m, ledger.Sum(x => x.FeedGram ?? 0m));
    }

    private static async Task<Fixture> CreateFixtureAsync(List<MalKabulVeSevkiyatDto>? currentRows = null, bool throwDuringSync = false)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AquaDbContext>().UseSqlite(connection).Options;
        var db = new ResyncSqliteAquaDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var unitOfWork = new EfUnitOfWork(db, new HttpContextAccessor());
        var service = new ErpReceiptResyncService(
            db,
            unitOfWork,
            new FakeNetsisReadService(currentRows ?? []),
            new FakeSyncJob(throwDuringSync),
            new LocalizationService(NullLogger<LocalizationService>.Instance));
        return new Fixture(connection, db, unitOfWork, service);
    }

    private static async Task<SeededGraph> SeedReceiptGraphAsync(AquaDbContext db, bool erpIntegratedFeeding, bool includeFeeding = true)
    {
        var receiptDate = new DateTime(2026, 7, 1);
        db.Users.Add(new User
        {
            Id = 1,
            Username = "resync-user",
            Email = "resync-user@example.com",
            PasswordHash = "test-hash",
            RoleId = 1,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var project = new Project { ProjectCode = "ERP-PRJ-001", ProjectName = "ERP Project", StartDate = receiptDate, Status = DocumentStatus.Posted };
        var cage = new Cage { CageCode = "B3", CageName = "B3 Cage" };
        var stock = new Stock { ErpStockCode = "FISH-001", StockName = "Levrek", Unit = "ADET", BranchCode = 1 };
        var feedStock = new Stock { ErpStockCode = "FEED-001", StockName = "Yem", Unit = "KG", GrupKodu = "YEM", BranchCode = 1 };
        db.AddRange(project, cage, stock, feedStock);
        await db.SaveChangesAsync();

        var projectCage = new ProjectCage { ProjectId = project.Id, CageId = cage.Id, AssignedDate = receiptDate };
        var receipt = new GoodsReceipt { ProjectId = project.Id, ReceiptNo = "ERP-GR-001", ReceiptDate = receiptDate, Status = DocumentStatus.Posted };
        db.AddRange(projectCage, receipt);
        await db.SaveChangesAsync();

        var batch = new FishBatch { ProjectId = project.Id, BatchCode = "ERP-GR-001", FishStockId = stock.Id, CurrentAverageGram = 100, StartDate = receiptDate };
        db.FishBatches.Add(batch);
        await db.SaveChangesAsync();

        var receiptLine = new GoodsReceiptLine
        {
            GoodsReceiptId = receipt.Id,
            ItemType = GoodsReceiptItemType.Fish,
            StockId = stock.Id,
            FishCount = 1_000,
            FishAverageGram = 100,
            FishTotalGram = 100_000,
            FishBatchId = batch.Id,
            ErpSourceMovementKey = "ERP-GR-001-LINE-1"
        };
        db.GoodsReceiptLines.Add(receiptLine);
        await db.SaveChangesAsync();

        db.AddRange(
            new GoodsReceiptFishDistribution { GoodsReceiptLineId = receiptLine.Id, ProjectCageId = projectCage.Id, FishBatchId = batch.Id, FishCount = 1_000 },
            new BatchCageBalance { FishBatchId = batch.Id, ProjectCageId = projectCage.Id, LiveCount = 1_000, AverageGram = 100, BiomassGram = 100_000, AsOfDate = receiptDate },
            new BatchMovement
            {
                FishBatchId = batch.Id,
                ProjectCageId = projectCage.Id,
                ToProjectCageId = projectCage.Id,
                ToStockId = stock.Id,
                ToAverageGram = 100,
                MovementDate = receiptDate,
                MovementType = BatchMovementType.Stocking,
                SignedCount = 1_000,
                SignedBiomassGram = 100_000,
                ReferenceTable = "RII_GOODS_RECEIPT_LINE",
                ReferenceId = receiptLine.Id
            });
        await db.SaveChangesAsync();

        db.ErpReceiptShipmentMovements.Add(new ErpReceiptShipmentMovement
        {
            SourceSystem = "Netsis",
            SourceMovementKey = "ERP-GR-001-LINE-1",
            MovementDate = receiptDate,
            DocumentNo = "ERP-GR-001",
            ErpWarehouseCode = 110,
            ErpProjectCode = project.ProjectCode,
            ErpStockCode = stock.ErpStockCode,
            Quantity = 1_000,
            MovementKind = "J",
            InOutCode = "G",
            OperationType = FishReceiptOperationType,
            ProjectId = project.Id,
            ProjectCageId = projectCage.Id,
            StockId = stock.Id,
            FishBatchId = batch.Id,
            GoodsReceiptId = receipt.Id,
            GoodsReceiptLineId = receiptLine.Id,
            IsMatched = true,
            IsProcessed = true,
            LastSyncedAt = receiptDate,
            ProcessedAt = receiptDate
        });

        if (includeFeeding)
        {
            var feeding = new Feeding
            {
                ProjectId = project.Id,
                FeedingNo = "FD-001",
                FeedingDate = receiptDate.AddDays(1),
                FeedingSlot = FeedingSlot.Morning,
                SourceType = FeedingSourceType.Manual,
                Status = DocumentStatus.Posted,
                IsERPIntegrated = erpIntegratedFeeding,
                ERPReferenceNumber = erpIntegratedFeeding ? "ERP-FEED-001" : null
            };
            db.Feedings.Add(feeding);
            await db.SaveChangesAsync();
            var feedingLine = new FeedingLine { FeedingId = feeding.Id, StockId = feedStock.Id, QtyUnit = 2.5m, GramPerUnit = 1_000, TotalGram = 2_500 };
            db.FeedingLines.Add(feedingLine);
            await db.SaveChangesAsync();
            var feedingDistribution = new FeedingDistribution { FeedingLineId = feedingLine.Id, FishBatchId = batch.Id, ProjectCageId = projectCage.Id, FeedGram = 2_500 };
            db.FeedingDistributions.Add(feedingDistribution);
            await db.SaveChangesAsync();
            db.BatchMovements.Add(new BatchMovement
            {
                FishBatchId = batch.Id,
                ProjectCageId = projectCage.Id,
                MovementDate = feeding.FeedingDate,
                MovementType = BatchMovementType.Feeding,
                SignedCount = 0,
                SignedBiomassGram = 0,
                FeedGram = 2_500,
                ReferenceTable = "RII_FEEDING_DISTRIBUTION",
                ReferenceId = feedingDistribution.Id
            });
        }

        await db.SaveChangesAsync();
        return new SeededGraph("ERP-GR-001", receiptDate, receipt.Id, batch.Id);
    }

    private sealed record SeededGraph(string DocumentNo, DateTime ReceiptDate, long GoodsReceiptId, long FishBatchId);

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly EfUnitOfWork _unitOfWork;

        public Fixture(SqliteConnection connection, AquaDbContext db, EfUnitOfWork unitOfWork, ErpReceiptResyncService service)
        {
            _connection = connection;
            _unitOfWork = unitOfWork;
            Db = db;
            Service = service;
        }

        public AquaDbContext Db { get; }
        public ErpReceiptResyncService Service { get; }

        public async ValueTask DisposeAsync()
        {
            _unitOfWork.Dispose();
            await _connection.DisposeAsync();
        }
    }

    private sealed class FakeSyncJob(bool shouldThrow) : IErpReceiptShipmentMovementSyncJob
    {
        public Task ExecuteAsync() => Task.CompletedTask;
        public Task ProcessMovementInCurrentTransactionAsync(MalKabulVeSevkiyatDto movement) =>
            shouldThrow ? Task.FromException(new InvalidOperationException("Simulated ERP reprocessing failure")) : Task.CompletedTask;
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

    private sealed class ResyncSqliteAquaDbContext(DbContextOptions<AquaDbContext> options) : AquaDbContext(options)
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
}
