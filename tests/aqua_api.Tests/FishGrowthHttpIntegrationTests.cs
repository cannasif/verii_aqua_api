using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using aqua_api.Modules.Aqua.Application.Services;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.AquaReports.Application.Dtos;
using aqua_api.Modules.FishGrowths.Application.Dtos;
using aqua_api.Modules.KpiReport.Application.Dtos;
using aqua_api.Modules.Mortalities.Application.Dtos;
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
            .DailyRows.Single(x => x.Date == "2026-07-01");
        Assert.Equal(1, dashboardGrowthDay.FishGrowthCount);
        Assert.NotEmpty(dashboardGrowthDay.FishGrowthDetails);
        Assert.Equal(0, dashboardGrowthDay.StockConvertCount);

        using var projectDetailResponse = await client.GetAsync($"/api/kpi-report/project-detail/{projectId}");
        Assert.Equal(HttpStatusCode.OK, projectDetailResponse.StatusCode);
        var projectDetailBody = await projectDetailResponse.Content.ReadFromJsonAsync<ApiResponse<ProjectDetailReportDto>>();
        var projectGrowthDay = projectDetailBody!.Data!.Cages.Single(x => x.ProjectCageId == projectCageId)
            .DailyRows.Single(x => x.Date == "2026-07-01");
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
    public async Task TargetGram_IsEffectiveForWholeMonth_AndAllReportsUseHistoricalWeight()
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
            var fishStockId = await db.Stocks.Where(x => x.ErpStockCode == "PLAMUT-5G").Select(x => x.Id).SingleAsync();
            var feedStockId = await db.Stocks.Where(x => x.ErpStockCode == "YEM-STD").Select(x => x.Id).SingleAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var project = new Project
            {
                ProjectCode = $"MONTH-GROW-{suffix}",
                ProjectName = "Monthly Target Gram Project",
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted
            };
            var cage = new Cage { CageCode = $"MC-{suffix}", CageName = "Monthly Growth Cage" };
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
                FishStockId = fishStockId,
                BatchCode = $"MB-{suffix}",
                CurrentAverageGram = 100m,
                StartDate = project.StartDate
            };
            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            await ledger.ApplyDelta(
                project.Id, batch.Id, projectCage.Id, 1_000, 100_000m,
                BatchMovementType.Stocking, project.StartDate, "Opening stocking", "TEST_OPENING", 1,
                null, projectCage.Id, fishStockId, fishStockId, 100m, 100m, 1);

            foreach (var (date, feedGram) in new[]
                     {
                         (new DateTime(2026, 1, 15), 50_000m),
                         (new DateTime(2026, 2, 15), 60_000m),
                         (new DateTime(2026, 3, 15), 70_000m)
                     })
            {
                var feeding = new Feeding
                {
                    ProjectId = project.Id,
                    FeedingNo = $"MONTH-FEED-{date:yyyyMM}-{suffix}",
                    FeedingDate = date,
                    FeedingSlot = FeedingSlot.Morning,
                    SourceType = FeedingSourceType.Manual,
                    Status = DocumentStatus.Posted
                };
                db.Feedings.Add(feeding);
                await db.SaveChangesAsync();

                var line = new FeedingLine
                {
                    FeedingId = feeding.Id,
                    StockId = feedStockId,
                    QtyUnit = feedGram / 1_000m,
                    GramPerUnit = 1_000m,
                    TotalGram = feedGram
                };
                db.FeedingLines.Add(line);
                await db.SaveChangesAsync();
                db.FeedingDistributions.Add(new FeedingDistribution
                {
                    FeedingLineId = line.Id,
                    FishBatchId = batch.Id,
                    ProjectCageId = projectCage.Id,
                    FeedGram = feedGram
                });
            }

            await db.SaveChangesAsync();
            projectId = project.Id;
            projectCageId = projectCage.Id;
            fishBatchId = batch.Id;
        }

        var februaryGrowth = await PostGrowth(new DateTime(2026, 2, 26), 200m);
        Assert.Equal(100m, februaryGrowth.PreviousAverageGram);
        Assert.Equal(100m, februaryGrowth.GrowthGram);
        Assert.Equal(200m, februaryGrowth.NewAverageGram);

        var marchGrowth = await PostGrowth(new DateTime(2026, 3, 3), 300m);
        Assert.Equal(200m, marchGrowth.PreviousAverageGram);
        Assert.Equal(100m, marchGrowth.GrowthGram);
        Assert.Equal(300m, marchGrowth.NewAverageGram);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var growthMovements = await db.BatchMovements
                .Where(x => x.FishBatchId == fishBatchId && x.MovementType == BatchMovementType.FishGrowth)
                .OrderBy(x => x.MovementDate)
                .ToListAsync();
            Assert.Collection(
                growthMovements,
                row =>
                {
                    Assert.Equal(new DateTime(2026, 2, 1), row.MovementDate);
                    Assert.Equal(100m, row.FromAverageGram);
                    Assert.Equal(200m, row.ToAverageGram);
                    Assert.Equal(100_000m, row.SignedBiomassGram);
                },
                row =>
                {
                    Assert.Equal(new DateTime(2026, 3, 1), row.MovementDate);
                    Assert.Equal(200m, row.FromAverageGram);
                    Assert.Equal(300m, row.ToAverageGram);
                    Assert.Equal(100_000m, row.SignedBiomassGram);
                });
        }

        var january = await GetDevirFcr(new DateTime(2026, 1, 31));
        Assert.Equal(1_000, january.EndingFishCount);
        Assert.Equal(100m, january.EndingAverageGram);
        Assert.Equal(100m, january.EndingBiomassKg);
        Assert.Equal(50m, january.TotalFeedKg);
        Assert.Equal(0.5m, january.Fcr);

        var february = await GetDevirFcr(new DateTime(2026, 2, 1));
        Assert.Equal(1_000, february.EndingFishCount);
        Assert.Equal(200m, february.EndingAverageGram);
        Assert.Equal(200m, february.EndingBiomassKg);

        var februaryEnd = await GetDevirFcr(new DateTime(2026, 2, 28));
        Assert.Equal(110m, februaryEnd.TotalFeedKg);
        Assert.Equal(0.55m, februaryEnd.Fcr);

        var march = await GetDevirFcr(new DateTime(2026, 3, 31));
        Assert.Equal(1_000, march.EndingFishCount);
        Assert.Equal(300m, march.EndingAverageGram);
        Assert.Equal(300m, march.EndingBiomassKg);
        Assert.Equal(180m, march.TotalFeedKg);
        Assert.Equal(0.6m, march.Fcr);

        using var dashboardResponse = await client.GetAsync($"/api/aqua/dashboard-project/detail/{projectId}");
        Assert.Equal(HttpStatusCode.OK, dashboardResponse.StatusCode);
        var dashboard = (await dashboardResponse.Content.ReadFromJsonAsync<ApiResponse<DashboardProjectDetailDto>>())!.Data!;
        var dashboardCage = Assert.Single(dashboard.Cages);
        Assert.Equal(1_000, dashboardCage.CurrentFishCount);
        Assert.Equal(300m, dashboardCage.CurrentAverageGram);
        Assert.Equal(300_000m, dashboardCage.CurrentBiomassGram);
        Assert.Equal(1, dashboardCage.DailyRows.Single(x => x.Date == "2026-02-01").FishGrowthCount);
        Assert.Equal(1, dashboardCage.DailyRows.Single(x => x.Date == "2026-03-01").FishGrowthCount);

        using var summaryResponse = await client.PostAsJsonAsync(
            "/api/aqua/dashboard-project/summary",
            new DashboardProjectsRequestDto { ProjectIds = [projectId] });
        Assert.Equal(HttpStatusCode.OK, summaryResponse.StatusCode);
        var summary = Assert.Single((await summaryResponse.Content.ReadFromJsonAsync<ApiResponse<DashboardProjectsResponseDto>>())!.Data!.Projects);
        Assert.Equal(1_000, summary.CageFish);
        Assert.Equal(300m, summary.MeasurementAverageGram);
        Assert.Equal(300_000m, summary.CageBiomassGram);
        Assert.Equal(0.6m, summary.Fcr);

        using var rawKpiResponse = await client.GetAsync($"/api/kpi-report/raw-kpi/{projectId}");
        Assert.Equal(HttpStatusCode.OK, rawKpiResponse.StatusCode);
        var rawKpi = (await rawKpiResponse.Content.ReadFromJsonAsync<ApiResponse<RawKpiReportDto>>())!.Data!;
        Assert.Equal(1_000, rawKpi.LiveFish);
        Assert.Equal(100m, rawKpi.InitialAverageGram);
        Assert.Equal(300m, rawKpi.CurrentAverageGram);
        Assert.Equal(300m, rawKpi.CurrentBiomassKg);
        Assert.Equal(0.9m, rawKpi.Fcr);

        using var projectDetailResponse = await client.GetAsync($"/api/kpi-report/project-detail/{projectId}");
        Assert.Equal(HttpStatusCode.OK, projectDetailResponse.StatusCode);
        var projectDetail = (await projectDetailResponse.Content.ReadFromJsonAsync<ApiResponse<ProjectDetailReportDto>>())!.Data!;
        var projectDetailCage = Assert.Single(projectDetail.Cages);
        Assert.Equal(1_000, projectDetailCage.CurrentFishCount);
        Assert.Equal(300m, projectDetailCage.CurrentAverageGram);
        Assert.Equal(300_000m, projectDetailCage.CurrentBiomassGram);
        Assert.Equal(1, projectDetailCage.DailyRows.Single(x => x.Date == "2026-02-01").FishGrowthCount);
        Assert.Equal(1, projectDetailCage.DailyRows.Single(x => x.Date == "2026-03-01").FishGrowthCount);

        using (var shipmentResponse = await client.PostAsJsonAsync("/api/aqua/ShipmentLine/auto-header", new CreateShipmentLineWithAutoHeaderDto
               {
                   ProjectId = projectId,
                   ShipmentDate = new DateTime(2026, 3, 20),
                   FishBatchId = fishBatchId,
                   FromProjectCageId = projectCageId,
                   FishCount = 100,
                   AverageGram = 1m,
                   BiomassGram = 1m
               }))
        {
            Assert.Equal(HttpStatusCode.OK, shipmentResponse.StatusCode);
            var shipmentBody = await shipmentResponse.Content.ReadFromJsonAsync<ApiResponse<ShipmentLineDto>>();
            Assert.True(shipmentBody?.Success, shipmentBody?.ExceptionMessage);
            Assert.Equal(300m, shipmentBody!.Data!.AverageGram);
            Assert.Equal(30_000m, shipmentBody.Data.BiomassGram);

            using var postResponse = await client.PostAsync($"/api/aqua/posting/shipment/{shipmentBody.Data.ShipmentId}", null);
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        }

        using (var mortalityResponse = await client.PostAsJsonAsync("/api/aqua/MortalityLine/auto-header", new CreateMortalityLineWithAutoHeaderDto
               {
                   ProjectId = projectId,
                   MortalityDate = new DateTime(2026, 3, 21),
                   FishBatchId = fishBatchId,
                   ProjectCageId = projectCageId,
                   DeadCount = 10
               }))
        {
            Assert.Equal(HttpStatusCode.OK, mortalityResponse.StatusCode);
            var mortalityBody = await mortalityResponse.Content.ReadFromJsonAsync<ApiResponse<MortalityLineDto>>();
            Assert.True(mortalityBody?.Success, mortalityBody?.ExceptionMessage);
        }

        var marchAfterOutputs = await GetDevirFcr(new DateTime(2026, 3, 31));
        Assert.Equal(890, marchAfterOutputs.EndingFishCount);
        Assert.Equal(300m, marchAfterOutputs.EndingAverageGram);
        Assert.Equal(267m, marchAfterOutputs.EndingBiomassKg);
        Assert.Equal(100, marchAfterOutputs.ShipmentFishCount);
        Assert.Equal(30m, marchAfterOutputs.ShippedBiomassKg);
        Assert.Equal(10, marchAfterOutputs.MortalityFishCount);
        Assert.Equal(3m, marchAfterOutputs.MortalityBiomassKg);
        Assert.Equal(0.6m, marchAfterOutputs.Fcr);

        var reportRequest = new MonthlyOperationalReportRequestDto
        {
            FromDate = new DateTime(2026, 3, 1),
            ToDate = new DateTime(2026, 3, 31),
            ProjectIds = [projectId],
            ProjectCageIds = [projectCageId]
        };
        using var shipmentReportResponse = await client.PostAsJsonAsync("/api/kpi-report/monthly-shipments", reportRequest);
        Assert.Equal(HttpStatusCode.OK, shipmentReportResponse.StatusCode);
        var shipmentReport = (await shipmentReportResponse.Content.ReadFromJsonAsync<ApiResponse<MonthlyOperationalReportDto>>())!.Data!;
        Assert.Equal(100, shipmentReport.TotalCount);
        Assert.Equal(30m, shipmentReport.TotalKg);

        using var mortalityReportResponse = await client.PostAsJsonAsync("/api/kpi-report/monthly-mortalities", reportRequest);
        Assert.Equal(HttpStatusCode.OK, mortalityReportResponse.StatusCode);
        var mortalityReport = (await mortalityReportResponse.Content.ReadFromJsonAsync<ApiResponse<MonthlyOperationalReportDto>>())!.Data!;
        Assert.Equal(10, mortalityReport.TotalCount);
        Assert.Equal(3m, mortalityReport.TotalKg);

        using var finalDashboardResponse = await client.GetAsync($"/api/aqua/dashboard-project/detail/{projectId}");
        var finalDashboard = (await finalDashboardResponse.Content.ReadFromJsonAsync<ApiResponse<DashboardProjectDetailDto>>())!.Data!;
        var finalDashboardCage = Assert.Single(finalDashboard.Cages);
        Assert.Equal(890, finalDashboardCage.CurrentFishCount);
        Assert.Equal(300m, finalDashboardCage.CurrentAverageGram);
        Assert.Equal(267_000m, finalDashboardCage.CurrentBiomassGram);

        using var finalRawKpiResponse = await client.GetAsync($"/api/kpi-report/raw-kpi/{projectId}");
        var finalRawKpi = (await finalRawKpiResponse.Content.ReadFromJsonAsync<ApiResponse<RawKpiReportDto>>())!.Data!;
        Assert.Equal(890, finalRawKpi.LiveFish);
        Assert.Equal(300m, finalRawKpi.CurrentAverageGram);
        Assert.Equal(267m, finalRawKpi.CurrentBiomassKg);
        Assert.Equal(10, finalRawKpi.DeadFish);

        async Task<FishGrowthDto> PostGrowth(DateTime growthDate, decimal targetGram)
        {
            using var response = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
            {
                ProjectId = projectId,
                ProjectCageId = projectCageId,
                FishBatchId = fishBatchId,
                GrowthDate = growthDate,
                NewAverageGram = targetGram
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
            Assert.True(body?.Success, body?.ExceptionMessage);
            return body!.Data!;
        }

        async Task<DevirFcrReportRowDto> GetDevirFcr(DateTime toDate)
        {
            using var response = await client.PostAsJsonAsync("/api/kpi-report/devir-fcr", new DevirFcrReportRequestDto
            {
                ProjectIds = [projectId],
                FromDate = new DateTime(2026, 1, 1),
                ToDate = toDate
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<DevirFcrReportDto>>();
            Assert.True(body?.Success, body?.ExceptionMessage);
            return Assert.Single(body!.Data!.Rows);
        }
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

        var januaryShipment = await PostShipment(new DateTime(2026, 1, 20));
        Assert.Equal(720m, januaryShipment.AverageGram);
        Assert.Equal(72_000m, januaryShipment.BiomassGram);

        using var lateJanuaryGrowthResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            GrowthDate = new DateTime(2026, 1, 25),
            NewAverageGram = 800m
        });
        Assert.Equal(HttpStatusCode.BadRequest, lateJanuaryGrowthResponse.StatusCode);
        var lateJanuaryGrowthBody = await lateJanuaryGrowthResponse.Content.ReadFromJsonAsync<ApiResponse<FishGrowthDto>>();
        Assert.Contains("büyütme", lateJanuaryGrowthBody!.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("satış", lateJanuaryGrowthBody.Message, StringComparison.OrdinalIgnoreCase);

        using var growthResponse = await client.PostAsJsonAsync("/api/aqua/FishGrowth", new CreateFishGrowthDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            GrowthDate = new DateTime(2026, 2, 20),
            NewAverageGram = 800m
        });
        Assert.Equal(HttpStatusCode.OK, growthResponse.StatusCode);

        var februaryShipment = await PostShipment(new DateTime(2026, 2, 5));
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
            using var postResponse = await client.PostAsync($"/api/aqua/posting/shipment/{body!.Data!.ShipmentId}", null);
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
            return body!.Data!;
        }
    }
}
