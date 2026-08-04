using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using aqua_api.Modules.Aqua.Application.Services;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.AquaReports.Application.Dtos;
using aqua_api.Modules.FishGrowths.Application.Dtos;
using aqua_api.Modules.KpiReport.Application.Dtos;
using aqua_api.Modules.Shipments.Application.Dtos;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Xunit;

namespace aqua_api.Tests;

public sealed class FishGrowthHttpIntegrationTests : IClassFixture<AquaHttpTestWebApplicationFactory>
{
    private readonly AquaHttpTestWebApplicationFactory _factory;

    public FishGrowthHttpIntegrationTests(AquaHttpTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_GrowsCurrentCageBatch_AndRejectsSecondGrowthInSameMonth()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        long projectId;
        long projectCageId;
        long fishBatchId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var stockId = await db.Stocks.Where(x => x.ErpStockCode == "PLAMUT-5G").Select(x => x.Id).SingleAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var project = new Project
            {
                ProjectCode = $"GROW-{suffix}",
                ProjectName = "Fish Growth Integration Project",
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted
            };
            var cage = new Cage { CageCode = $"GC-{suffix}", CageName = "Growth Cage" };
            db.Projects.Add(project);
            db.Cages.Add(cage);
            await db.SaveChangesAsync();

            var projectCage = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cage.Id,
                AssignedDate = project.StartDate
            };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                FishStockId = stockId,
                BatchCode = $"GB-{suffix}",
                CurrentAverageGram = 720m,
                StartDate = project.StartDate
            };
            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            db.BatchCageBalances.Add(new BatchCageBalance
            {
                ProjectCageId = projectCage.Id,
                FishBatchId = batch.Id,
                LiveCount = 1_000,
                AverageGram = 720m,
                BiomassGram = 720_000m,
                AsOfDate = project.StartDate
            });
            await db.SaveChangesAsync();

            projectId = project.Id;
            projectCageId = projectCage.Id;
            fishBatchId = batch.Id;
        }

        var request = new CreateFishGrowthDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            GrowthDate = new DateTime(2026, 7, 15),
            NewAverageGram = 840m
        };

        using var firstResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", request);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var firstBody = await firstResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
        Assert.True(firstBody?.Success, firstBody?.ExceptionMessage);
        Assert.Equal(720m, firstBody!.Data!.PreviousAverageGram);
        Assert.Equal(120m, firstBody.Data.GrowthGram);
        Assert.Equal(16.6667m, firstBody.Data.GrowthRatePercent);
        Assert.Equal(840m, firstBody.Data.NewAverageGram);
        Assert.Equal(1_000, firstBody.Data.FishCount);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var balance = await db.BatchCageBalances.SingleAsync(x => x.ProjectCageId == projectCageId && x.FishBatchId == fishBatchId);
            Assert.Equal(1_000, balance.LiveCount);
            Assert.Equal(840m, balance.AverageGram);
            Assert.Equal(840_000m, balance.BiomassGram);

            var movement = await db.BatchMovements.SingleAsync(x => x.ReferenceTable == "RII_FISH_GROWTH" && x.ReferenceId == firstBody.Data.Id);
            Assert.Equal(BatchMovementType.FishGrowth, movement.MovementType);
            Assert.Equal(0, movement.SignedCount);
            Assert.Equal(120_000m, movement.SignedBiomassGram);
        }

        using var dashboardResponse = await client.GetAsync($"/api/aqua/dashboard-project/detail/{projectId}");
        Assert.Equal(HttpStatusCode.OK, dashboardResponse.StatusCode);
        var dashboardBody = await dashboardResponse.Content.ReadFromJsonAsync<ApiResponse<DashboardProjectDetailDto>>();
        var dashboardGrowthDay = dashboardBody!.Data!.Cages.Single(x => x.ProjectCageId == projectCageId)
            .DailyRows.Single(x => x.Date == "2026-07-15");
        Assert.Equal(1, dashboardGrowthDay.FishGrowthCount);
        Assert.NotEmpty(dashboardGrowthDay.FishGrowthDetails);
        Assert.Equal(0, dashboardGrowthDay.StockConvertCount);

        using var projectDetailResponse = await client.GetAsync($"/api/kpi-report/project-detail/{projectId}");
        Assert.Equal(HttpStatusCode.OK, projectDetailResponse.StatusCode);
        var projectDetailBody = await projectDetailResponse.Content.ReadFromJsonAsync<ApiResponse<ProjectDetailReportDto>>();
        var projectGrowthDay = projectDetailBody!.Data!.Cages.Single(x => x.ProjectCageId == projectCageId)
            .DailyRows.Single(x => x.Date == "2026-07-15");
        Assert.Equal(1, projectGrowthDay.FishGrowthCount);
        Assert.NotEmpty(projectGrowthDay.FishGrowthDetails);
        Assert.Equal(0, projectGrowthDay.StockConvertCount);

        request.GrowthDate = new DateTime(2026, 7, 28);
        request.NewAverageGram = 850m;
        using var duplicateResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", request);
        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
        var duplicateBody = await duplicateResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
        Assert.False(duplicateBody?.Success);
        Assert.Equal(
            "Bu kafes ve balık partisi için seçilen ayda büyütme kaydı zaten bulunmaktadır.",
            duplicateBody!.Message);
        Assert.Equal(duplicateBody.Message, duplicateBody.ExceptionMessage);

        using var deleteResponse = await client.DeleteAsync($"/api/aqua/FishGrowth/{firstBody.Data.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        var deleteBody = await deleteResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        Assert.True(deleteBody?.Success);
        Assert.True(deleteBody!.Data);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var revertedBalance = await db.BatchCageBalances.SingleAsync(x =>
                x.ProjectCageId == projectCageId && x.FishBatchId == fishBatchId);
            Assert.Equal(720m, revertedBalance.AverageGram);
            Assert.Equal(720_000m, revertedBalance.BiomassGram);
            Assert.Empty(await db.FishGrowths.Where(x => x.Id == firstBody.Data.Id).ToListAsync());
            Assert.Empty(await db.BatchMovements.Where(x =>
                x.ReferenceTable == "RII_FISH_GROWTH" && x.ReferenceId == firstBody.Data.Id).ToListAsync());
            Assert.True(await db.FishGrowths.IgnoreQueryFilters().AnyAsync(x =>
                x.Id == firstBody.Data.Id && x.IsDeleted));
            Assert.True(await db.BatchMovements.IgnoreQueryFilters().AnyAsync(x =>
                x.ReferenceTable == "RII_FISH_GROWTH" && x.ReferenceId == firstBody.Data.Id && x.IsDeleted));
        }

        request.NewAverageGram = 730m;
        using var recreateResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", request);
        Assert.Equal(HttpStatusCode.OK, recreateResponse.StatusCode);
        var recreateBody = await recreateResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
        Assert.True(recreateBody?.Success, recreateBody?.ExceptionMessage);
        Assert.Equal(730m, recreateBody!.Data!.NewAverageGram);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var ledger = scope.ServiceProvider.GetRequiredService<IBalanceLedgerManager>();
            await ledger.ApplyDelta(
                projectId,
                fishBatchId,
                projectCageId,
                0,
                100m,
                BatchMovementType.Adjustment,
                new DateTime(2026, 7, 29),
                "Later balance movement",
                "TEST_LATER_MOVEMENT",
                1,
                projectCageId,
                projectCageId,
                null,
                null,
                730m,
                730.1m,
                1);
            await db.SaveChangesAsync();
        }

        using var blockedDeleteResponse = await client.DeleteAsync($"/api/aqua/FishGrowth/{recreateBody.Data.Id}");
        Assert.Equal(HttpStatusCode.BadRequest, blockedDeleteResponse.StatusCode);
        var blockedDeleteBody = await blockedDeleteResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        Assert.Contains("sonra satış, fire, transfer", blockedDeleteBody!.Message);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        Assert.Equal(1, await verifyDb.FishGrowths.CountAsync(x => x.ProjectCageId == projectCageId && x.FishBatchId == fishBatchId));
    }

    [Fact]
    public async Task Shipment_UsesExitWeightAtShipmentDate_AndIgnoresClientWeight()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        long projectId;
        long projectCageId;
        long fishBatchId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var ledger = scope.ServiceProvider.GetRequiredService<IBalanceLedgerManager>();
            var stockId = await db.Stocks.Where(x => x.ErpStockCode == "PLAMUT-5G").Select(x => x.Id).SingleAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var project = new Project
            {
                ProjectCode = $"SHIP-GROW-{suffix}",
                ProjectName = "Shipment Weight Snapshot Project",
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted
            };
            var cage = new Cage { CageCode = $"SC-{suffix}", CageName = "Shipment Snapshot Cage" };
            db.Projects.Add(project);
            db.Cages.Add(cage);
            await db.SaveChangesAsync();

            var projectCage = new ProjectCage { ProjectId = project.Id, CageId = cage.Id, AssignedDate = project.StartDate };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                FishStockId = stockId,
                BatchCode = $"SB-{suffix}",
                CurrentAverageGram = 720m,
                StartDate = project.StartDate
            };
            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            await ledger.ApplyDelta(
                project.Id, batch.Id, projectCage.Id, 1_000, 720_000m,
                BatchMovementType.Stocking, project.StartDate, "Opening stocking", "TEST_OPENING", 1,
                null, projectCage.Id, stockId, stockId, 720m, 720m, 1);
            await db.SaveChangesAsync();

            projectId = project.Id;
            projectCageId = projectCage.Id;
            fishBatchId = batch.Id;
        }

        using var growthResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            GrowthDate = new DateTime(2026, 2, 1),
            NewAverageGram = 800m
        });
        Assert.Equal(HttpStatusCode.OK, growthResponse.StatusCode);

        var januaryShipment = await PostShipment(new DateTime(2026, 1, 20));
        Assert.Equal(720m, januaryShipment.AverageGram);
        Assert.Equal(72_000m, januaryShipment.BiomassGram);

        var februaryShipment = await PostShipment(new DateTime(2026, 2, 20));
        Assert.Equal(800m, februaryShipment.AverageGram);
        Assert.Equal(80_000m, februaryShipment.BiomassGram);

        async Task<ShipmentLineDto> PostShipment(DateTime shipmentDate)
        {
            using var response = await client.PostAsJsonAsync("/api/aqua/ShipmentLine/auto-header", new CreateShipmentLineWithAutoHeaderDto
            {
                ProjectId = projectId,
                ShipmentDate = shipmentDate,
                FishBatchId = fishBatchId,
                FromProjectCageId = projectCageId,
                FishCount = 100,
                AverageGram = 1m,
                BiomassGram = 1m
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<ShipmentLineDto>>();
            Assert.True(body?.Success, body?.ExceptionMessage);
            return body!.Data!;
        }
    }
}
