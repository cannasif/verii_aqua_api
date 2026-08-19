using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.AquaReports.Application.Dtos;
using aqua_api.Modules.KpiReport.Application.Dtos;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Xunit;

namespace aqua_api.Tests;

public sealed class MortalityLineDeletionHttpIntegrationTests : IClassFixture<AquaHttpTestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly AquaHttpTestWebApplicationFactory _factory;

    public MortalityLineDeletionHttpIntegrationTests(AquaHttpTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeletePostedLines_SoftDeletesMovements_ReplaysBalances_AndDeletesEmptyHeader()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        long projectId;
        long batchId;
        long projectCageAId;
        long projectCageBId;
        long mortalityId;
        long lineAId;
        long lineBId;
        long movementAId;
        long movementBId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var fishStockId = await db.Stocks
                .Where(x => !x.IsDeleted && x.ErpStockCode == "PLAMUT-5G")
                .Select(x => x.Id)
                .SingleAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var project = new Project
            {
                ProjectCode = $"MORT-DEL-{suffix}",
                ProjectName = $"Mortality deletion {suffix}",
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted,
                CreatedBy = 1,
            };
            var cageA = new Cage { CageCode = $"MD-A-{suffix}", CageName = "Mortality deletion A", CreatedBy = 1 };
            var cageB = new Cage { CageCode = $"MD-B-{suffix}", CageName = "Mortality deletion B", CreatedBy = 1 };
            db.Projects.Add(project);
            db.Cages.AddRange(cageA, cageB);
            await db.SaveChangesAsync();

            var projectCageA = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cageA.Id,
                AssignedDate = new DateTime(2026, 1, 1),
                CreatedBy = 1,
            };
            var projectCageB = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cageB.Id,
                AssignedDate = new DateTime(2026, 1, 1),
                CreatedBy = 1,
            };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                BatchCode = $"MD-BATCH-{suffix}",
                FishStockId = fishStockId,
                CurrentAverageGram = 100m,
                StartDate = new DateTime(2026, 1, 1),
                CreatedBy = 1,
            };
            db.ProjectCages.AddRange(projectCageA, projectCageB);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            var mortality = new Mortality
            {
                ProjectId = project.Id,
                MortalityNo = $"MORT-{suffix}",
                MortalityDate = new DateTime(2026, 1, 10),
                Status = DocumentStatus.Posted,
                IsERPIntegrated = false,
                ERPIntegrationStatus = "Pending",
                CreatedBy = 1,
            };
            db.Mortalities.Add(mortality);
            await db.SaveChangesAsync();

            var lineA = new MortalityLine
            {
                MortalityId = mortality.Id,
                FishBatchId = batch.Id,
                ProjectCageId = projectCageA.Id,
                DeadCount = 60,
                CreatedBy = 1,
            };
            var lineB = new MortalityLine
            {
                MortalityId = mortality.Id,
                FishBatchId = batch.Id,
                ProjectCageId = projectCageB.Id,
                DeadCount = 40,
                CreatedBy = 1,
            };
            var openingA = Movement(batch.Id, projectCageA.Id, new DateTime(2026, 1, 1), BatchMovementType.Stocking, 1_000, 100_000m, "TEST_OPENING", 1);
            var openingB = Movement(batch.Id, projectCageB.Id, new DateTime(2026, 1, 1), BatchMovementType.Stocking, 1_000, 100_000m, "TEST_OPENING", 2);
            var mortalityA = Movement(batch.Id, projectCageA.Id, mortality.MortalityDate, BatchMovementType.Mortality, -60, -6_000m, "RII_MORTALITY", mortality.Id, 100m);
            var mortalityB = Movement(batch.Id, projectCageB.Id, mortality.MortalityDate, BatchMovementType.Mortality, -40, -4_000m, "RII_MORTALITY", mortality.Id, 100m);
            db.MortalityLines.AddRange(lineA, lineB);
            db.BatchMovements.AddRange(openingA, openingB, mortalityA, mortalityB);
            db.BatchCageBalances.AddRange(
                Balance(batch.Id, projectCageA.Id, 940, 94_000m, mortality.MortalityDate),
                Balance(batch.Id, projectCageB.Id, 960, 96_000m, mortality.MortalityDate));
            await db.SaveChangesAsync();

            projectId = project.Id;
            batchId = batch.Id;
            projectCageAId = projectCageA.Id;
            projectCageBId = projectCageB.Id;
            mortalityId = mortality.Id;
            lineAId = lineA.Id;
            lineBId = lineB.Id;
            movementAId = mortalityA.Id;
            movementBId = mortalityB.Id;
        }

        await DeleteOk(client, $"/api/aqua/MortalityLine/{lineAId}");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            Assert.True((await db.MortalityLines.IgnoreQueryFilters().SingleAsync(x => x.Id == lineAId)).IsDeleted);
            Assert.False((await db.Mortalities.IgnoreQueryFilters().SingleAsync(x => x.Id == mortalityId)).IsDeleted);
            Assert.True((await db.BatchMovements.IgnoreQueryFilters().SingleAsync(x => x.Id == movementAId)).IsDeleted);
            Assert.False((await db.BatchMovements.IgnoreQueryFilters().SingleAsync(x => x.Id == movementBId)).IsDeleted);
            Assert.False(await db.BatchMovements.AnyAsync(x => !x.IsDeleted && x.ReferenceTable == "RII_MORTALITY" && x.ReferenceId == mortalityId && x.ProjectCageId == projectCageAId));
            await AssertBalance(db, batchId, projectCageAId, 1_000, 100_000m);
            await AssertBalance(db, batchId, projectCageBId, 960, 96_000m);
        }

        var oneLineReport = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-mortalities", Range(projectId));
        Assert.Equal(40, oneLineReport.TotalCount);

        await DeleteOk(client, $"/api/aqua/MortalityLine/{lineBId}");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            Assert.True((await db.MortalityLines.IgnoreQueryFilters().SingleAsync(x => x.Id == lineBId)).IsDeleted);
            Assert.True((await db.Mortalities.IgnoreQueryFilters().SingleAsync(x => x.Id == mortalityId)).IsDeleted);
            Assert.True((await db.BatchMovements.IgnoreQueryFilters().SingleAsync(x => x.Id == movementBId)).IsDeleted);
            Assert.False(await db.BatchMovements.AnyAsync(x => !x.IsDeleted && x.ReferenceTable == "RII_MORTALITY" && x.ReferenceId == mortalityId));
            await AssertBalance(db, batchId, projectCageAId, 1_000, 100_000m);
            await AssertBalance(db, batchId, projectCageBId, 1_000, 100_000m);
        }

        var emptyReport = await PostOk<MonthlyOperationalReportDto>(client, "/api/kpi-report/monthly-mortalities", Range(projectId));
        Assert.Equal(0, emptyReport.TotalCount);
        Assert.Equal(0m, emptyReport.TotalKg);

        var devir = await PostOk<DevirFcrReportDto>(client, "/api/kpi-report/devir-fcr", new DevirFcrReportRequestDto
        {
            ProjectIds = [projectId],
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 1, 31),
        });
        var row = Assert.Single(devir.Rows);
        Assert.Equal(2_000, row.OpeningFishCount);
        Assert.Equal(0, row.MortalityFishCount);
        Assert.Equal(2_000, row.EndingFishCount);
        Assert.Equal(200m, row.EndingBiomassKg);
    }

    private static BatchMovement Movement(
        long batchId,
        long cageId,
        DateTime date,
        BatchMovementType type,
        int count,
        decimal biomassGram,
        string referenceTable,
        long referenceId,
        decimal? averageGram = null) => new()
        {
            FishBatchId = batchId,
            ProjectCageId = cageId,
            FromProjectCageId = cageId,
            MovementDate = date,
            MovementType = type,
            SignedCount = count,
            SignedBiomassGram = biomassGram,
            FromAverageGram = averageGram,
            ToAverageGram = averageGram,
            ReferenceTable = referenceTable,
            ReferenceId = referenceId,
            ActorUserId = 1,
            CreatedBy = 1,
        };

    private static BatchCageBalance Balance(long batchId, long cageId, int count, decimal biomassGram, DateTime date) => new()
    {
        FishBatchId = batchId,
        ProjectCageId = cageId,
        LiveCount = count,
        AverageGram = biomassGram / count,
        BiomassGram = biomassGram,
        AsOfDate = date,
        CreatedBy = 1,
    };

    private static async Task AssertBalance(AquaDbContext db, long batchId, long cageId, int count, decimal biomassGram)
    {
        var balance = await db.BatchCageBalances.SingleAsync(x =>
            !x.IsDeleted && x.FishBatchId == batchId && x.ProjectCageId == cageId);
        Assert.Equal(count, balance.LiveCount);
        Assert.Equal(biomassGram, balance.BiomassGram);
        Assert.Equal(100m, balance.AverageGram);
    }

    private static MonthlyOperationalReportRequestDto Range(long projectId) => new()
    {
        ProjectIds = [projectId],
        FromDate = new DateTime(2026, 1, 1),
        ToDate = new DateTime(2026, 1, 31),
    };

    private static async Task<T> PostOk<T>(HttpClient client, string url, object payload)
    {
        using var response = await client.PostAsJsonAsync(url, payload);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        Assert.NotNull(body);
        Assert.True(response.IsSuccessStatusCode, $"{url}: HTTP {(int)response.StatusCode} | {body!.Message} | {body.ExceptionMessage}");
        Assert.True(body!.Success, $"{url}: {body.Message} | {body.ExceptionMessage}");
        Assert.NotNull(body.Data);
        return body.Data!;
    }

    private static async Task DeleteOk(HttpClient client, string url)
    {
        using var response = await client.DeleteAsync(url);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(JsonOptions);
        Assert.NotNull(body);
        Assert.True(response.IsSuccessStatusCode, $"{url}: HTTP {(int)response.StatusCode} | {body!.Message} | {body.ExceptionMessage}");
        Assert.True(body!.Success, $"{url}: {body.Message} | {body.ExceptionMessage}");
        Assert.True(body.Data);
    }
}
