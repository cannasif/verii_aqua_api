using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using aqua_api.Modules.Aqua.Application.Dtos;
using aqua_api.Modules.Aqua.Domain.Entities;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Xunit;

namespace aqua_api.Tests;

public sealed class AquaSqlServerProviderParityTests : IClassFixture<AquaSqlServerParityWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AquaSqlServerParityWebApplicationFactory _factory;

    public AquaSqlServerProviderParityTests(AquaSqlServerParityWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SqlServerProviderParity_LifecycleEndpointsStayConsistent()
    {
        if (!string.IsNullOrWhiteSpace(_factory.UnavailableReason))
        {
            Console.WriteLine($"SQL Server parity test skipped: {_factory.UnavailableReason}");
            return;
        }

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var stock5g = await db.Stocks.SingleAsync(x => x.ErpStockCode == "PLAMUT-5G");
        var stock10g = await db.Stocks.SingleAsync(x => x.ErpStockCode == "PLAMUT-10G");
        var feedStock = await db.Stocks.SingleAsync(x => x.ErpStockCode == "YEM-STD");
        var project = new Project
        {
            ProjectCode = "SQL-PARITY",
            ProjectName = "SQL Provider Parity Project",
            StartDate = new DateTime(2026, 4, 1),
            Status = DocumentStatus.Posted,
        };
        var cage = new Cage
        {
            CageCode = "SQL-CAGE-1",
            CageName = "SQL Cage 1",
            CapacityCount = 20_000,
            CapacityGram = 200_000m,
        };

        db.Projects.Add(project);
        db.Cages.Add(cage);
        await db.SaveChangesAsync();

        var projectCage = new ProjectCage
        {
            ProjectId = project.Id,
            CageId = cage.Id,
            AssignedDate = new DateTime(2026, 4, 1),
        };
        db.ProjectCages.Add(projectCage);
        await db.SaveChangesAsync();

        var fishBatch = new FishBatch
        {
            ProjectId = project.Id,
            BatchCode = "SQL-BATCH-001",
            FishStockId = stock5g.Id,
            CurrentAverageGram = 5m,
            StartDate = new DateTime(2026, 4, 1),
            TargetHarvestAverageGram = 10m,
            TargetHarvestDate = new DateTime(2026, 4, 30),
        };
        db.FishBatches.Add(fishBatch);
        await db.SaveChangesAsync();

        db.BatchCageBalances.Add(new BatchCageBalance
        {
            FishBatchId = fishBatch.Id,
            ProjectCageId = projectCage.Id,
            LiveCount = 5_000,
            AverageGram = 5m,
            BiomassGram = 25_000m,
            AsOfDate = new DateTime(2026, 4, 1),
        });
        await db.SaveChangesAsync();

        await AquaHttpTestWebApplicationFactory.SeedFeedPurchaseHistoryAsync(db, project.Id, 1, feedStock.Id);

        var warehouse = await db.Warehouses.SingleAsync(x => x.ErpWarehouseCode == 10);
        var client = _factory.CreateClient();

        var feeding = await PostAsync<FeedingLineDto>(client, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            FeedingDate = new DateTime(2026, 4, 2),
            FeedingSlot = FeedingSlot.Morning,
            SourceType = FeedingSourceType.Manual,
            StockId = feedStock.Id,
            QtyUnit = 1,
            GramPerUnit = 12_000m,
            TotalGram = 12_000m,
        });

        Assert.True(feeding.Success, $"{feeding.Message} | {feeding.ExceptionMessage}");

        var convertedBatch = new FishBatch
        {
            ProjectId = project.Id,
            BatchCode = "SQL-BATCH-001-10G",
            FishStockId = stock10g.Id,
            CurrentAverageGram = 10m,
            StartDate = new DateTime(2026, 4, 3),
        };
        db.FishBatches.Add(convertedBatch);
        await db.SaveChangesAsync();

        var stockConvert = await PostAsync<StockConvertLineDto>(client, "/api/aqua/StockConvertLine/auto-header", new CreateStockConvertLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            ConvertDate = new DateTime(2026, 4, 3),
            FromFishBatchId = fishBatch.Id,
            ToFishBatchId = convertedBatch.Id,
            FromProjectCageId = projectCage.Id,
            ToProjectCageId = projectCage.Id,
            FishCount = 1_000,
            AverageGram = 5m,
            NewAverageGram = 10m,
            BiomassGram = 10_000m,
        });

        Assert.True(stockConvert.Success, $"{stockConvert.Message} | {stockConvert.ExceptionMessage}");

        var shipment = await PostAsync<ShipmentLineDto>(client, "/api/aqua/ShipmentLine/auto-header", new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = project.Id,
            ShipmentDate = new DateTime(2026, 4, 4),
            TargetWarehouseId = warehouse.Id,
            FishBatchId = convertedBatch.Id,
            FromProjectCageId = projectCage.Id,
            FishCount = 500,
            AverageGram = 10m,
            BiomassGram = 5_000m,
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 210m,
            LocalUnitPrice = 210m,
            LineAmount = 1_050m,
            LocalLineAmount = 1_050m,
        });

        Assert.True(shipment.Success, $"{shipment.Message} | {shipment.ExceptionMessage}");

        var postedShipment = await client.PostAsync($"/api/aqua/AquaPosting/shipment/{shipment.Data!.ShipmentId}", null);
        Assert.True(postedShipment.IsSuccessStatusCode);

        var snapshot = await client.PostAsync($"/api/aqua/ProjectCageDailyKpi/project-cages/{projectCage.Id}/snapshot?date=2026-04-04", null);
        Assert.True(snapshot.IsSuccessStatusCode);

        var latest = await GetAsync<ProjectCageDailyKpiSnapshotDto>(client, $"/api/aqua/ProjectCageDailyKpi/project-cages/{projectCage.Id}/latest");
        Assert.True(latest.Success, $"{latest.Message} | {latest.ExceptionMessage}");
        Assert.True(latest.Data!.SurvivalPct > 0);

        using var balanceResponse = await client.GetAsync($"/api/aqua/BatchCageBalance/getall?pageIndex=1&pageSize=20&projectCageId={projectCage.Id}");
        var balanceJson = await balanceResponse.Content.ReadAsStringAsync();
        Assert.True(balanceResponse.IsSuccessStatusCode);
        Assert.Contains("SQL-BATCH-001", balanceJson);
    }

    private static async Task<ApiResponse<T>> PostAsync<T>(HttpClient client, string url, object payload)
    {
        using var response = await client.PostAsJsonAsync(url, payload);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        Assert.NotNull(body);
        return body!;
    }

    private static async Task<ApiResponse<T>> GetAsync<T>(HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        Assert.NotNull(body);
        return body!;
    }
}
