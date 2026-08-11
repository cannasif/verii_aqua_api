using System.Net.Http.Json;
using System.Text.Json;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.AquaSettings.Application.Dtos;
using aqua_api.Modules.AquaReports.Application.Dtos;
using aqua_api.Modules.BatchBalances.Domain.Entities;
using aqua_api.Modules.KpiReport.Application.Dtos;
using aqua_api.Modules.Stock.Domain.Entities;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace aqua_api.Tests;

public sealed class MortalityBiomassModeHttpIntegrationTests
    : IClassFixture<AquaHttpTestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly AquaHttpTestWebApplicationFactory _factory;

    public MortalityBiomassModeHttpIntegrationTests(AquaHttpTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Setting_ChangesMortalityKgConsistentlyAcrossAllReports()
    {
        var seeded = await SeedScenarioAsync();
        using var client = _factory.CreateClient();
        var range = new MonthlyOperationalReportRequestDto
        {
            ProjectIds = [seeded.ProjectId],
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 3, 31),
        };

        await UpdateSettingsAsync(client, mortalityMode: 0);
        await AssertSettingsReadIsFreshAsync(client, expectedMode: 0);
        await AssertReportValuesAsync(client, seeded, range, expectedKg: 3m, expectedGram: 3_000m);

        await UpdateSettingsAsync(client, mortalityMode: 1);
        await AssertSettingsReadIsFreshAsync(client, expectedMode: 1);
        await AssertReportValuesAsync(client, seeded, range, expectedKg: 4.5m, expectedGram: 4_500m);
    }

    private async Task<SeededScenario> SeedScenarioAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var fishStock = await db.Stocks.FirstAsync(x => !x.IsDeleted && x.Unit == "ADET");

        var project = new Project
        {
            ProjectCode = "MORT-MODE-TEST",
            ProjectName = "Mortality mode report test",
            StartDate = new DateTime(2026, 1, 1),
            Status = DocumentStatus.Posted,
        };
        var cageA = new Cage { CageCode = "MMA", CageName = "Mode A" };
        var cageB = new Cage { CageCode = "MMB", CageName = "Mode B" };
        db.AddRange(project, cageA, cageB);
        await db.SaveChangesAsync();

        var projectCageA = new ProjectCage
        {
            ProjectId = project.Id,
            CageId = cageA.Id,
            AssignedDate = project.StartDate,
        };
        var projectCageB = new ProjectCage
        {
            ProjectId = project.Id,
            CageId = cageB.Id,
            AssignedDate = project.StartDate,
        };
        var batchA = Batch(project.Id, fishStock, "MORT-A", 300m);
        var batchB = Batch(project.Id, fishStock, "MORT-B", 600m);
        db.AddRange(projectCageA, projectCageB, batchA, batchB);
        await db.SaveChangesAsync();

        var mortalityA = Mortality(project.Id, "MORT-A-01", new DateTime(2026, 1, 15));
        var mortalityB = Mortality(project.Id, "MORT-B-01", new DateTime(2026, 2, 15));
        db.AddRange(mortalityA, mortalityB);
        await db.SaveChangesAsync();

        db.MortalityLines.AddRange(
            new MortalityLine
            {
                MortalityId = mortalityA.Id,
                FishBatchId = batchA.Id,
                ProjectCageId = projectCageA.Id,
                DeadCount = 10,
            },
            new MortalityLine
            {
                MortalityId = mortalityB.Id,
                FishBatchId = batchB.Id,
                ProjectCageId = projectCageB.Id,
                DeadCount = 10,
            });

        db.BatchMovements.AddRange(
            Movement(batchA.Id, projectCageA.Id, new DateTime(2026, 1, 1), BatchMovementType.Stocking, 100, 100m, "OPEN-A", 1),
            Movement(batchA.Id, projectCageA.Id, mortalityA.MortalityDate, BatchMovementType.Mortality, -10, 100m, "RII_MORTALITY", mortalityA.Id),
            Growth(batchA.Id, projectCageA.Id, new DateTime(2026, 3, 1), 100m, 300m),
            Movement(batchB.Id, projectCageB.Id, new DateTime(2026, 1, 1), BatchMovementType.Stocking, 100, 500m, "OPEN-B", 1),
            Movement(batchB.Id, projectCageB.Id, mortalityB.MortalityDate, BatchMovementType.Mortality, -10, 500m, "RII_MORTALITY", mortalityB.Id),
            Growth(batchB.Id, projectCageB.Id, new DateTime(2026, 3, 1), 500m, 600m));
        await db.SaveChangesAsync();

        return new SeededScenario(project.Id, projectCageA.Id, projectCageB.Id);
    }

    private static FishBatch Batch(long projectId, Stock stock, string code, decimal currentAverageGram)
    {
        return new FishBatch
        {
            ProjectId = projectId,
            FishStockId = stock.Id,
            BatchCode = code,
            CurrentAverageGram = currentAverageGram,
            StartDate = new DateTime(2026, 1, 1),
        };
    }

    private static Mortality Mortality(long projectId, string no, DateTime date)
    {
        return new Mortality
        {
            ProjectId = projectId,
            MortalityNo = no,
            MortalityDate = date,
            Status = DocumentStatus.Posted,
        };
    }

    private static BatchMovement Movement(
        long fishBatchId,
        long projectCageId,
        DateTime date,
        BatchMovementType type,
        int count,
        decimal averageGram,
        string referenceTable,
        long referenceId)
    {
        return new BatchMovement
        {
            FishBatchId = fishBatchId,
            ProjectCageId = projectCageId,
            FromProjectCageId = count < 0 ? projectCageId : null,
            ToProjectCageId = count > 0 ? projectCageId : null,
            MovementDate = date,
            MovementType = type,
            SignedCount = count,
            SignedBiomassGram = count * averageGram,
            FromAverageGram = averageGram,
            ToAverageGram = averageGram,
            ReferenceTable = referenceTable,
            ReferenceId = referenceId,
            CreatedDate = date,
        };
    }

    private static BatchMovement Growth(
        long fishBatchId,
        long projectCageId,
        DateTime date,
        decimal fromAverageGram,
        decimal toAverageGram)
    {
        return new BatchMovement
        {
            FishBatchId = fishBatchId,
            ProjectCageId = projectCageId,
            FromProjectCageId = projectCageId,
            ToProjectCageId = projectCageId,
            MovementDate = date,
            MovementType = BatchMovementType.FishGrowth,
            SignedCount = 0,
            SignedBiomassGram = 90m * (toAverageGram - fromAverageGram),
            FromAverageGram = fromAverageGram,
            ToAverageGram = toAverageGram,
            ReferenceTable = "RII_FISH_GROWTH",
            ReferenceId = fishBatchId,
            CreatedDate = date,
        };
    }

    private static async Task UpdateSettingsAsync(HttpClient client, int mortalityMode)
    {
        await PostAsync<AquaSettingsDto>(client, "/api/aqua/AquaSettings/update", new UpdateAquaSettingsDto
        {
            RequireFullTransfer = true,
            AllowProjectMerge = false,
            PartialTransferOccupiedCageMode = 0,
            FeedCostFallbackStrategy = 0,
            MortalityBiomassCalculationMode = mortalityMode,
        });
    }

    private static async Task AssertReportValuesAsync(
        HttpClient client,
        SeededScenario seeded,
        MonthlyOperationalReportRequestDto range,
        decimal expectedKg,
        decimal expectedGram)
    {
        var monthly = await PostAsync<MonthlyOperationalReportDto>(
            client,
            "/api/kpi-report/monthly-mortalities",
            range);
        Assert.Equal(20, monthly.TotalCount);
        Assert.Equal(expectedKg, monthly.TotalKg);

        var devir = await PostAsync<DevirFcrReportDto>(client, "/api/kpi-report/devir-fcr", new DevirFcrReportRequestDto
        {
            ProjectIds = [seeded.ProjectId],
            FromDate = range.FromDate!.Value,
            ToDate = range.ToDate!.Value,
        });
        Assert.Equal(expectedKg, Assert.Single(devir.Rows).MortalityBiomassKg);

        var dashboard = await PostAsync<DashboardProjectsResponseDto>(
            client,
            "/api/aqua/dashboard-project/summary",
            new DashboardProjectsRequestDto { ProjectIds = [seeded.ProjectId] });
        Assert.Equal(expectedGram, Assert.Single(dashboard.Projects).TotalDeadBiomassGram);

        var dashboardDetail = await GetAsync<DashboardProjectDetailDto>(
            client,
            $"/api/aqua/dashboard-project/detail/{seeded.ProjectId}");
        Assert.Equal(expectedGram, dashboardDetail.Cages.Sum(x => x.TotalDeadBiomassGram));

        var projectDetail = await GetAsync<ProjectDetailReportDto>(
            client,
            $"/api/kpi-report/project-detail/{seeded.ProjectId}");
        Assert.Equal(
            expectedGram,
            projectDetail.Cages.SelectMany(x => x.DailyRows).Sum(x => x.DeadBiomassGram));
        Assert.Contains(projectDetail.Cages, x => x.ProjectCageId == seeded.ProjectCageAId);
        Assert.Contains(projectDetail.Cages, x => x.ProjectCageId == seeded.ProjectCageBId);
    }

    private static async Task AssertSettingsReadIsFreshAsync(HttpClient client, int expectedMode)
    {
        using var response = await client.GetAsync("/api/aqua/AquaSettings");
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AquaSettingsDto>>(JsonOptions);

        Assert.True(response.IsSuccessStatusCode && payload?.Success == true);
        Assert.Equal(expectedMode, Assert.IsType<AquaSettingsDto>(payload!.Data).MortalityBiomassCalculationMode);
        Assert.True(response.Headers.CacheControl?.NoStore == true);
    }

    private static async Task<T> PostAsync<T>(HttpClient client, string url, object body)
    {
        using var response = await client.PostAsJsonAsync(url, body);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        Assert.True(
            response.IsSuccessStatusCode && payload?.Success == true,
            $"HTTP {(int)response.StatusCode}: {payload?.Message} | {payload?.ExceptionMessage}");
        return Assert.IsType<T>(payload!.Data);
    }

    private static async Task<T> GetAsync<T>(HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        Assert.True(
            response.IsSuccessStatusCode && payload?.Success == true,
            $"HTTP {(int)response.StatusCode}: {payload?.Message} | {payload?.ExceptionMessage}");
        return Assert.IsType<T>(payload!.Data);
    }

    private sealed record SeededScenario(long ProjectId, long ProjectCageAId, long ProjectCageBId);
}
