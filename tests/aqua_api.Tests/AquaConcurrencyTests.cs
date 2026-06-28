using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Xunit;

namespace aqua_api.Tests;

public sealed class AquaConcurrencyTests : IClassFixture<AquaConcurrencyHttpTestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AquaConcurrencyHttpTestWebApplicationFactory _factory;

    public AquaConcurrencyTests(AquaConcurrencyHttpTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FeedingLineAutoHeader_BlocksExistingQuickEntrySlotAndKeepsManualUpdateFlow()
    {
        var clientA = _factory.CreateClient();
        var clientB = _factory.CreateClient();
        clientA.DefaultRequestHeaders.Add("X-Branch-Code", "1");
        clientB.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var projectCode = $"PRJ-CON-{suffix}";
        var cageCode = $"CAGE-CON-{suffix}";
        var batchCode = $"BATCH-CON-{suffix}";

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(clientA, "/api/aqua/OpeningImport/preview", new OpeningImportPreviewRequestDto
        {
            FileName = "concurrency-opening.xlsx",
            SourceSystem = "concurrency-test",
            Sheets =
            [
                new OpeningImportSheetPayloadDto
                {
                    SheetName = "Projects",
                    Mappings =
                    [
                        new OpeningImportFieldMappingDto { SourceColumn = "projectCode", TargetField = "projectCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "projectName", TargetField = "projectName" },
                        new OpeningImportFieldMappingDto { SourceColumn = "startDate", TargetField = "startDate" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode,
                            ["projectName"] = "Concurrency Farm",
                            ["startDate"] = "2026-04-01",
                        }
                    ]
                },
                new OpeningImportSheetPayloadDto
                {
                    SheetName = "Cages",
                    Mappings =
                    [
                        new OpeningImportFieldMappingDto { SourceColumn = "projectCode", TargetField = "projectCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "cageCode", TargetField = "cageCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "cageName", TargetField = "cageName" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode,
                            ["cageCode"] = cageCode,
                            ["cageName"] = "Concurrency Cage",
                        }
                    ]
                },
                new OpeningImportSheetPayloadDto
                {
                    SheetName = "OpeningStock",
                    Mappings =
                    [
                        new OpeningImportFieldMappingDto { SourceColumn = "projectCode", TargetField = "projectCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "cageCode", TargetField = "cageCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "batchCode", TargetField = "batchCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "fishStockCode", TargetField = "fishStockCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "fishCount", TargetField = "fishCount" },
                        new OpeningImportFieldMappingDto { SourceColumn = "averageGram", TargetField = "averageGram" },
                        new OpeningImportFieldMappingDto { SourceColumn = "asOfDate", TargetField = "asOfDate" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode,
                            ["cageCode"] = cageCode,
                            ["batchCode"] = batchCode,
                            ["fishStockCode"] = "PLAMUT-5G",
                            ["fishCount"] = "10000",
                            ["averageGram"] = "5",
                            ["asOfDate"] = "2026-04-01",
                        }
                    ]
                }
            ]
        });
        Assert.True(preview.Success);

        var commit = await PostAsync<OpeningImportCommitResultDto>(clientA, $"/api/aqua/OpeningImport/{preview.Data!.JobId}/commit", new { });
        Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");

        long projectId;
        long feedStockId;
        long alternateFeedStockId;
        long projectCageId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            projectId = await db.Projects.Where(x => !x.IsDeleted && x.ProjectCode == projectCode).Select(x => x.Id).SingleAsync();
            feedStockId = await db.Stocks.Where(x => !x.IsDeleted && x.ErpStockCode == "YEM-STD").Select(x => x.Id).SingleAsync();
            projectCageId = await db.ProjectCages
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Cage != null && x.Cage.CageCode == cageCode)
                .Select(x => x.Id)
                .SingleAsync();

            var alternateFeedStock = new aqua_api.Modules.Stock.Domain.Entities.Stock
            {
                ErpStockCode = $"YEM-ALT-{suffix}",
                StockName = "Alternatif Yem",
                Unit = "KG",
                GrupKodu = "YEM",
                GrupAdi = "Yem",
                IsDeleted = false,
            };
            db.Stocks.Add(alternateFeedStock);
            await db.SaveChangesAsync();
            alternateFeedStockId = alternateFeedStock.Id;
        }

        var date = new DateTime(2026, 4, 6);
        var taskA = PostAsync<FeedingLineDto>(clientA, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            FeedingDate = date,
            FeedingSlot = FeedingSlot.Morning,
            StockId = feedStockId,
            QtyUnit = 10m,
            GramPerUnit = 1000m,
            TotalGram = 10_000m,
        });
        var taskB = PostAsync<FeedingLineDto>(clientB, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            FeedingDate = date,
            FeedingSlot = FeedingSlot.Morning,
            StockId = feedStockId,
            QtyUnit = 12m,
            GramPerUnit = 1000m,
            TotalGram = 12_000m,
        });

        var results = await Task.WhenAll(taskA, taskB);
        Assert.Single(results, result => result.Success);
        var rejectedMorning = Assert.Single(results, result => !result.Success);
        Assert.Equal(400, rejectedMorning.StatusCode);
        Assert.Contains("Hızlı giriş", rejectedMorning.Message);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var headers = await verifyDb.Feedings
            .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.FeedingDate.Date == date.Date && x.FeedingSlot == FeedingSlot.Morning)
            .ToListAsync();
        var lines = await verifyDb.FeedingLines
            .Where(x => !x.IsDeleted && headers.Select(h => h.Id).Contains(x.FeedingId))
            .ToListAsync();

        Assert.Single(headers);
        var line = Assert.Single(lines);
        Assert.Equal(feedStockId, line.StockId);
        Assert.Contains(line.QtyUnit, new[] { 10m, 12m });
        Assert.Equal(line.QtyUnit * 1000m, line.TotalGram);
        var morningQty = line.QtyUnit;

        var evening = await PostAsync<FeedingLineDto>(clientA, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            FeedingDate = date,
            FeedingSlot = FeedingSlot.Evening,
            StockId = feedStockId,
            QtyUnit = 5m,
            GramPerUnit = 1000m,
            TotalGram = 5_000m,
        });
        Assert.True(evening.Success, $"{evening.Message} | {evening.ExceptionMessage}");

        var allHeaders = await verifyDb.Feedings
            .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.FeedingDate.Date == date.Date)
            .ToListAsync();
        var allHeaderIds = allHeaders.Select(x => x.Id).ToList();
        var allLines = await verifyDb.FeedingLines
            .Where(x => !x.IsDeleted && allHeaderIds.Contains(x.FeedingId))
            .ToListAsync();

        Assert.Equal(2, allHeaders.Count);
        Assert.Equal(2, allLines.Count);
        Assert.Equal(morningQty * 1000m, allLines.Single(x => x.FeedingId == headers.Single().Id).TotalGram);
        Assert.Equal(5_000m, allLines.Single(x => x.FeedingId != headers.Single().Id).TotalGram);

        var manualLine = await PostAsync<FeedingLineDto>(clientA, "/api/aqua/FeedingLine", new CreateFeedingLineDto
        {
            FeedingId = headers.Single().Id,
            StockId = feedStockId,
            QtyUnit = 3m,
            GramPerUnit = 1000m,
            TotalGram = 3_000m,
        });
        Assert.True(manualLine.Success, $"{manualLine.Message} | {manualLine.ExceptionMessage}");

        using var manualVerifyScope = _factory.Services.CreateScope();
        var manualVerifyDb = manualVerifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var manualLines = await manualVerifyDb.FeedingLines
            .Where(x => !x.IsDeleted && x.FeedingId == headers.Single().Id)
            .ToListAsync();

        var mergedManualLine = Assert.Single(manualLines);
        Assert.Equal(feedStockId, mergedManualLine.StockId);
        Assert.Equal(morningQty + 3m, mergedManualLine.QtyUnit);
        Assert.Equal((morningQty + 3m) * 1000m, mergedManualLine.TotalGram);

        var cageScopedDate = new DateTime(2026, 4, 7);
        var cageScopedFirst = await PostAsync<FeedingLineDto>(clientA, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FeedingDate = cageScopedDate,
            FeedingSlot = FeedingSlot.Morning,
            StockId = feedStockId,
            QtyUnit = 10m,
            GramPerUnit = 1000m,
            TotalGram = 10_000m,
        });
        Assert.True(cageScopedFirst.Success, $"{cageScopedFirst.Message} | {cageScopedFirst.ExceptionMessage}");

        var cageScopedSecond = await PostAsync<FeedingLineDto>(clientA, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FeedingDate = cageScopedDate,
            FeedingSlot = FeedingSlot.Morning,
            StockId = alternateFeedStockId,
            QtyUnit = 12m,
            GramPerUnit = 1000m,
            TotalGram = 12_000m,
        });
        Assert.False(cageScopedSecond.Success);
        Assert.Equal(400, cageScopedSecond.StatusCode);
        Assert.Contains("Hızlı giriş", cageScopedSecond.Message);

        using var cageScopedVerifyScope = _factory.Services.CreateScope();
        var cageScopedVerifyDb = cageScopedVerifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var cageScopedHeader = await cageScopedVerifyDb.Feedings
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == projectId && x.FeedingDate.Date == cageScopedDate.Date && x.FeedingSlot == FeedingSlot.Morning);
        var cageScopedLines = await cageScopedVerifyDb.FeedingLines
            .Where(x => !x.IsDeleted && x.FeedingId == cageScopedHeader.Id)
            .ToListAsync();
        var cageScopedDistributions = await cageScopedVerifyDb.FeedingDistributions
            .Where(x => !x.IsDeleted && x.ProjectCageId == projectCageId && cageScopedLines.Select(l => l.Id).Contains(x.FeedingLineId))
            .ToListAsync();

        var cageScopedLine = Assert.Single(cageScopedLines);
        Assert.Equal(feedStockId, cageScopedLine.StockId);
        Assert.Equal(10m, cageScopedLine.QtyUnit);
        Assert.Equal(10_000m, cageScopedLine.TotalGram);
        var cageScopedDistribution = Assert.Single(cageScopedDistributions);
        Assert.Equal(10_000m, cageScopedDistribution.FeedGram);
    }

    [Fact]
    public async Task MortalityLineAutoHeader_AllowsSameBatchAcrossDifferentCagesOnSameDate()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var projectCode = $"PRJ-MORT-{suffix}";
        var cageCodeA = $"MORT-A-{suffix}";
        var cageCodeB = $"MORT-B-{suffix}";
        var batchCode = $"BATCH-MORT-{suffix}";

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(client, "/api/aqua/OpeningImport/preview", new OpeningImportPreviewRequestDto
        {
            FileName = "mortality-multi-cage-opening.xlsx",
            SourceSystem = "mortality-multi-cage-test",
            Sheets =
            [
                new OpeningImportSheetPayloadDto
                {
                    SheetName = "Projects",
                    Mappings =
                    [
                        new OpeningImportFieldMappingDto { SourceColumn = "projectCode", TargetField = "projectCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "projectName", TargetField = "projectName" },
                        new OpeningImportFieldMappingDto { SourceColumn = "startDate", TargetField = "startDate" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode,
                            ["projectName"] = "Mortality Multi Cage Project",
                            ["startDate"] = "2026-04-01",
                        }
                    ]
                },
                new OpeningImportSheetPayloadDto
                {
                    SheetName = "Cages",
                    Mappings =
                    [
                        new OpeningImportFieldMappingDto { SourceColumn = "projectCode", TargetField = "projectCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "cageCode", TargetField = "cageCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "cageName", TargetField = "cageName" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode,
                            ["cageCode"] = cageCodeA,
                            ["cageName"] = "Mortality Cage A",
                        },
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode,
                            ["cageCode"] = cageCodeB,
                            ["cageName"] = "Mortality Cage B",
                        }
                    ]
                },
                new OpeningImportSheetPayloadDto
                {
                    SheetName = "OpeningStock",
                    Mappings =
                    [
                        new OpeningImportFieldMappingDto { SourceColumn = "projectCode", TargetField = "projectCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "cageCode", TargetField = "cageCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "batchCode", TargetField = "batchCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "fishStockCode", TargetField = "fishStockCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "fishCount", TargetField = "fishCount" },
                        new OpeningImportFieldMappingDto { SourceColumn = "averageGram", TargetField = "averageGram" },
                        new OpeningImportFieldMappingDto { SourceColumn = "asOfDate", TargetField = "asOfDate" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode,
                            ["cageCode"] = cageCodeA,
                            ["batchCode"] = batchCode,
                            ["fishStockCode"] = "PLAMUT-5G",
                            ["fishCount"] = "1000",
                            ["averageGram"] = "5",
                            ["asOfDate"] = "2026-04-01",
                        },
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode,
                            ["cageCode"] = cageCodeB,
                            ["batchCode"] = batchCode,
                            ["fishStockCode"] = "PLAMUT-5G",
                            ["fishCount"] = "2000",
                            ["averageGram"] = "5",
                            ["asOfDate"] = "2026-04-01",
                        }
                    ]
                }
            ]
        });
        Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");

        var commit = await PostAsync<OpeningImportCommitResultDto>(client, $"/api/aqua/OpeningImport/{preview.Data!.JobId}/commit", new { });
        Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");

        long projectId;
        long fishBatchId;
        long projectCageIdA;
        long projectCageIdB;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            projectId = await db.Projects.Where(x => !x.IsDeleted && x.ProjectCode == projectCode).Select(x => x.Id).SingleAsync();
            fishBatchId = await db.FishBatches.Where(x => !x.IsDeleted && x.BatchCode == batchCode).Select(x => x.Id).SingleAsync();
            projectCageIdA = await db.ProjectCages
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Cage != null && x.Cage.CageCode == cageCodeA)
                .Select(x => x.Id)
                .SingleAsync();
            projectCageIdB = await db.ProjectCages
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Cage != null && x.Cage.CageCode == cageCodeB)
                .Select(x => x.Id)
                .SingleAsync();
        }

        var mortalityDate = new DateTime(2026, 4, 10);
        var cageAMortality = await PostAsync<MortalityLineDto>(client, "/api/aqua/MortalityLine/auto-header", new CreateMortalityLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            MortalityDate = mortalityDate,
            FishBatchId = fishBatchId,
            ProjectCageId = projectCageIdA,
            DeadCount = 10,
        });
        Assert.True(cageAMortality.Success, $"{cageAMortality.Message} | {cageAMortality.ExceptionMessage}");

        var cageBMortality = await PostAsync<MortalityLineDto>(client, "/api/aqua/MortalityLine/auto-header", new CreateMortalityLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            MortalityDate = mortalityDate,
            FishBatchId = fishBatchId,
            ProjectCageId = projectCageIdB,
            DeadCount = 20,
        });
        Assert.True(cageBMortality.Success, $"{cageBMortality.Message} | {cageBMortality.ExceptionMessage}");

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var mortalityHeader = await verifyDb.Mortalities
            .SingleAsync(x => !x.IsDeleted && x.ProjectId == projectId && x.MortalityDate.Date == mortalityDate.Date);
        var mortalityLines = await verifyDb.MortalityLines
            .Where(x => !x.IsDeleted && x.MortalityId == mortalityHeader.Id)
            .OrderBy(x => x.ProjectCageId)
            .ToListAsync();
        var mortalityMovements = await verifyDb.BatchMovements
            .Where(x =>
                !x.IsDeleted &&
                x.ReferenceTable == "RII_MORTALITY" &&
                x.ReferenceId == mortalityHeader.Id &&
                x.MovementType == BatchMovementType.Mortality)
            .OrderBy(x => x.ProjectCageId)
            .ToListAsync();
        var balances = await verifyDb.BatchCageBalances
            .Where(x => !x.IsDeleted && x.FishBatchId == fishBatchId && (x.ProjectCageId == projectCageIdA || x.ProjectCageId == projectCageIdB))
            .ToDictionaryAsync(x => x.ProjectCageId);

        Assert.Equal(DocumentStatus.Posted, mortalityHeader.Status);
        Assert.False(mortalityHeader.IsERPIntegrated);
        Assert.Equal("Pending", mortalityHeader.ERPIntegrationStatus);
        Assert.Equal(2, mortalityLines.Count);
        Assert.Contains(mortalityLines, x => x.ProjectCageId == projectCageIdA && x.DeadCount == 10);
        Assert.Contains(mortalityLines, x => x.ProjectCageId == projectCageIdB && x.DeadCount == 20);
        Assert.Equal(2, mortalityMovements.Count);
        Assert.Contains(mortalityMovements, x => x.ProjectCageId == projectCageIdA && x.SignedCount == -10);
        Assert.Contains(mortalityMovements, x => x.ProjectCageId == projectCageIdB && x.SignedCount == -20);
        Assert.Equal(990, balances[projectCageIdA].LiveCount);
        Assert.Equal(1980, balances[projectCageIdB].LiveCount);
    }

    [Fact]
    public async Task QuickDailyEntry_AllowsThreeDaysOfFeedingAndMortalityForSameBatchAcrossDifferentCages()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var projectCode = $"20220617OLIVKA-{suffix}";
        var cageCodeA = $"B3-{suffix}";
        var cageCodeB = $"B4-{suffix}";
        var batchCode = $"BATCH-OLIVKA-{suffix}";

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(client, "/api/aqua/OpeningImport/preview", new OpeningImportPreviewRequestDto
        {
            FileName = "olivka-same-batch-multi-cage-opening.xlsx",
            SourceSystem = "olivka-same-batch-multi-cage-test",
            Sheets =
            [
                new OpeningImportSheetPayloadDto
                {
                    SheetName = "Projects",
                    Mappings =
                    [
                        new OpeningImportFieldMappingDto { SourceColumn = "projectCode", TargetField = "projectCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "projectName", TargetField = "projectName" },
                        new OpeningImportFieldMappingDto { SourceColumn = "startDate", TargetField = "startDate" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode,
                            ["projectName"] = "12. PROJE",
                            ["startDate"] = "2026-04-01",
                        }
                    ]
                },
                new OpeningImportSheetPayloadDto
                {
                    SheetName = "Cages",
                    Mappings =
                    [
                        new OpeningImportFieldMappingDto { SourceColumn = "projectCode", TargetField = "projectCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "cageCode", TargetField = "cageCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "cageName", TargetField = "cageName" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode,
                            ["cageCode"] = cageCodeA,
                            ["cageName"] = "B3 Kafes",
                        },
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode,
                            ["cageCode"] = cageCodeB,
                            ["cageName"] = "B4 Kafes",
                        }
                    ]
                },
                new OpeningImportSheetPayloadDto
                {
                    SheetName = "OpeningStock",
                    Mappings =
                    [
                        new OpeningImportFieldMappingDto { SourceColumn = "projectCode", TargetField = "projectCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "cageCode", TargetField = "cageCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "batchCode", TargetField = "batchCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "fishStockCode", TargetField = "fishStockCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "fishCount", TargetField = "fishCount" },
                        new OpeningImportFieldMappingDto { SourceColumn = "averageGram", TargetField = "averageGram" },
                        new OpeningImportFieldMappingDto { SourceColumn = "asOfDate", TargetField = "asOfDate" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode,
                            ["cageCode"] = cageCodeA,
                            ["batchCode"] = batchCode,
                            ["fishStockCode"] = "PLAMUT-5G",
                            ["fishCount"] = "1000",
                            ["averageGram"] = "1530",
                            ["asOfDate"] = "2026-04-01",
                        },
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode,
                            ["cageCode"] = cageCodeB,
                            ["batchCode"] = batchCode,
                            ["fishStockCode"] = "PLAMUT-5G",
                            ["fishCount"] = "1500",
                            ["averageGram"] = "1530",
                            ["asOfDate"] = "2026-04-01",
                        }
                    ]
                }
            ]
        });
        Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");

        var commit = await PostAsync<OpeningImportCommitResultDto>(client, $"/api/aqua/OpeningImport/{preview.Data!.JobId}/commit", new { });
        Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");

        long projectId;
        long fishBatchId;
        long feedStockId;
        long projectCageIdA;
        long projectCageIdB;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            projectId = await db.Projects.Where(x => !x.IsDeleted && x.ProjectCode == projectCode).Select(x => x.Id).SingleAsync();
            fishBatchId = await db.FishBatches.Where(x => !x.IsDeleted && x.BatchCode == batchCode).Select(x => x.Id).SingleAsync();
            feedStockId = await db.Stocks.Where(x => !x.IsDeleted && x.ErpStockCode == "YEM-STD").Select(x => x.Id).SingleAsync();
            projectCageIdA = await db.ProjectCages
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Cage != null && x.Cage.CageCode == cageCodeA)
                .Select(x => x.Id)
                .SingleAsync();
            projectCageIdB = await db.ProjectCages
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.Cage != null && x.Cage.CageCode == cageCodeB)
                .Select(x => x.Id)
                .SingleAsync();
        }

        var dates = new[]
        {
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 11),
            new DateTime(2026, 4, 12),
        };

        foreach (var date in dates)
        {
            await AssertQuickDailyFeedingAndMortalityAsync(client, projectId, fishBatchId, feedStockId, projectCageIdA, date, feedKg: 2, deadCount: 1);
        }

        foreach (var date in dates)
        {
            await AssertQuickDailyFeedingAndMortalityAsync(client, projectId, fishBatchId, feedStockId, projectCageIdB, date, feedKg: 3, deadCount: 2);
        }

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var feedingHeaders = await verifyDb.Feedings
            .Where(x => !x.IsDeleted && x.ProjectId == projectId && dates.Contains(x.FeedingDate.Date))
            .ToListAsync();
        var mortalityHeaders = await verifyDb.Mortalities
            .Where(x => !x.IsDeleted && x.ProjectId == projectId && dates.Contains(x.MortalityDate.Date))
            .ToListAsync();
        var feedingDistributions = await verifyDb.FeedingDistributions
            .Where(x =>
                !x.IsDeleted &&
                x.FishBatchId == fishBatchId &&
                (x.ProjectCageId == projectCageIdA || x.ProjectCageId == projectCageIdB))
            .ToListAsync();
        var mortalityLines = await verifyDb.MortalityLines
            .Where(x =>
                !x.IsDeleted &&
                x.FishBatchId == fishBatchId &&
                (x.ProjectCageId == projectCageIdA || x.ProjectCageId == projectCageIdB))
            .ToListAsync();
        var balances = await verifyDb.BatchCageBalances
            .Where(x => !x.IsDeleted && x.FishBatchId == fishBatchId && (x.ProjectCageId == projectCageIdA || x.ProjectCageId == projectCageIdB))
            .ToDictionaryAsync(x => x.ProjectCageId);

        Assert.Equal(3, feedingHeaders.Count);
        Assert.Equal(3, mortalityHeaders.Count);
        Assert.Equal(6, feedingDistributions.Count);
        Assert.Equal(6, mortalityLines.Count);
        Assert.Equal(997, balances[projectCageIdA].LiveCount);
        Assert.Equal(1494, balances[projectCageIdB].LiveCount);
        Assert.All(mortalityHeaders, x =>
        {
            Assert.Equal(DocumentStatus.Posted, x.Status);
            Assert.False(x.IsERPIntegrated);
            Assert.Equal("Pending", x.ERPIntegrationStatus);
        });
    }

    private static async Task AssertQuickDailyFeedingAndMortalityAsync(
        HttpClient client,
        long projectId,
        long fishBatchId,
        long feedStockId,
        long projectCageId,
        DateTime date,
        decimal feedKg,
        int deadCount)
    {
        var feeding = await PostAsync<FeedingLineDto>(client, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            ProjectCageId = projectCageId,
            FishBatchId = fishBatchId,
            FeedingDate = date,
            FeedingSlot = FeedingSlot.Morning,
            StockId = feedStockId,
            QtyUnit = feedKg,
            GramPerUnit = 1000m,
            TotalGram = feedKg * 1000m,
        });
        Assert.True(feeding.Success, $"{feeding.Message} | {feeding.ExceptionMessage}");

        var mortality = await PostAsync<MortalityLineDto>(client, "/api/aqua/MortalityLine/auto-header", new CreateMortalityLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            MortalityDate = date,
            FishBatchId = fishBatchId,
            ProjectCageId = projectCageId,
            DeadCount = deadCount,
        });
        Assert.True(mortality.Success, $"{mortality.Message} | {mortality.ExceptionMessage}");
    }

    private static async Task<ApiResponse<T>> PostAsync<T>(HttpClient client, string url, object payload)
    {
        using var response = await client.PostAsJsonAsync(url, payload);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        Assert.NotNull(body);
        return body!;
    }
}
