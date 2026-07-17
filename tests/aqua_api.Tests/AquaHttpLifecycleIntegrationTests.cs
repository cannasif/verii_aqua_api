using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using aqua_api.Modules.Aqua.Application.Services;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Xunit;

namespace aqua_api.Tests;

public sealed class AquaHttpLifecycleIntegrationTests : IClassFixture<AquaHttpTestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AquaHttpTestWebApplicationFactory _factory;

    public AquaHttpLifecycleIntegrationTests(AquaHttpTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void OpeningImport_ParsesCustomerExcelNumberFormats()
    {
        Assert.Equal(7686, InvokeParseInt("7.686"));
        Assert.Equal(477175, InvokeParseInt("477175.00"));
        Assert.Equal(6481, InvokeParseInt("6,481.27"));
        Assert.Equal(986203m, InvokeParseDecimal("986,203.00"));
        Assert.Equal(174906.7143m, InvokeParseDecimal("174906.7143"));
    }

    [Fact]
    public async Task OpeningImport_PreviewBlocksExistingProjectAndCage_AndCommitDoesNotRun()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var existingProject = new Project
            {
                ProjectCode = "PRJ-DUP-001",
                ProjectName = "Existing Project",
                StartDate = new DateTime(2026, 4, 1),
                Status = DocumentStatus.Posted,
            };
            var existingCage = new Cage
            {
                CageCode = "CAGE-DUP-01",
                CageName = "Existing Cage",
            };

            db.Projects.Add(existingProject);
            db.Cages.Add(existingCage);
            await db.SaveChangesAsync();
        }

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(client, "/api/aqua/OpeningImport/preview", new OpeningImportPreviewRequestDto
        {
            FileName = "duplicate-opening-import.xlsx",
            SourceSystem = "http-duplicate-test",
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
                            ["projectCode"] = "PRJ-DUP-001",
                            ["projectName"] = "Duplicate Project",
                            ["startDate"] = "2026-04-05",
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
                            ["projectCode"] = "PRJ-DUP-001",
                            ["cageCode"] = "CAGE-DUP-01",
                            ["cageName"] = "Duplicate Cage",
                        }
                    ]
                }
            ]
        });

        Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
        Assert.NotNull(preview.Data);
        Assert.Equal("Failed", preview.Data!.Status);
        Assert.Contains(preview.Data.Rows, row => row.Messages.Any(message => message.Contains("Proje zaten sistemde kayıtlı")));
        Assert.Contains(preview.Data.Rows, row => row.Messages.Any(message => message.Contains("Aynı proje için kafes kodu zaten tanımlı")));

        using var commitResponse = await client.PostAsJsonAsync($"/api/aqua/OpeningImport/{preview.Data.JobId}/commit", new { });
        Assert.Equal(HttpStatusCode.BadRequest, commitResponse.StatusCode);

        var commitBody = await commitResponse.Content.ReadFromJsonAsync<ApiResponse<OpeningImportCommitResultDto>>(JsonOptions);
        Assert.NotNull(commitBody);
        Assert.False(commitBody!.Success);
    }

    [Fact]
    public async Task OpeningImport_PreviewBlocksSoftDeletedProjectAndCageCodes()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var deletedProject = new Project
            {
                ProjectCode = "PRJ-DELETED-001",
                ProjectName = "Deleted Test Project",
                StartDate = new DateTime(2026, 4, 1),
                Status = DocumentStatus.Draft,
                IsDeleted = true
            };
            var deletedCage = new Cage
            {
                CageCode = "CAGE-DELETED-001",
                CageName = "Deleted Test Cage",
                IsDeleted = true
            };
            db.Projects.Add(deletedProject);
            db.Cages.Add(deletedCage);
            db.ProjectCages.Add(new ProjectCage
            {
                Project = deletedProject,
                Cage = deletedCage,
                AssignedDate = new DateTime(2026, 4, 1),
            });
            await db.SaveChangesAsync();
        }

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(client, "/api/aqua/OpeningImport/preview", new OpeningImportPreviewRequestDto
        {
            FileName = "soft-deleted-opening-import.xlsx",
            SourceSystem = "soft-deleted-test",
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
                            ["projectCode"] = "PRJ-DELETED-001",
                            ["projectName"] = "Live Project Reusing Deleted Test Code",
                            ["startDate"] = "2026-04-05",
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
                            ["projectCode"] = "PRJ-DELETED-001",
                            ["cageCode"] = "CAGE-DELETED-001",
                            ["cageName"] = "Live Cage Reusing Deleted Test Code",
                        }
                    ]
                }
            ]
        });

        Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
        Assert.NotNull(preview.Data);
        Assert.Equal("Failed", preview.Data!.Status);
        Assert.Contains(preview.Data.Rows, row => row.Messages.Any(message => message.Contains("Proje daha önce silinmiş")));
        Assert.Contains(preview.Data.Rows, row => row.Messages.Any(message => message.Contains("Silinmiş bir kafes kodu tekrar girilmiş")));

        var cleanup = await PostAsync<OpeningImportCleanupSoftDeletedResultDto>(client, $"/api/aqua/OpeningImport/{preview.Data.JobId}/cleanup-soft-deleted", new { });
        Assert.True(cleanup.Success, $"{cleanup.Message} | {cleanup.ExceptionMessage}");
        Assert.NotNull(cleanup.Data);
        Assert.Equal(1, cleanup.Data!.DeletedProjects);
        Assert.Equal(1, cleanup.Data.DeletedCages);
        Assert.Equal(1, cleanup.Data.DeletedProjectCages);
        Assert.Contains("PRJ-DELETED-001", cleanup.Data.DeletedProjectCodes);
        Assert.Contains("CAGE-DELETED-001", cleanup.Data.DeletedCageCodes);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        Assert.False(await verifyDb.Projects.IgnoreQueryFilters().AnyAsync(x => x.ProjectCode == "PRJ-DELETED-001"));
        Assert.False(await verifyDb.Cages.IgnoreQueryFilters().AnyAsync(x => x.CageCode == "CAGE-DELETED-001"));
    }

    [Fact]
    public async Task OpeningImport_ResetExistingDataHardDeletesActiveDemoProjectAndCageScope()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var project = new Project
            {
                ProjectCode = "PRJ-RESET-001",
                ProjectName = "Reset Demo Project",
                StartDate = new DateTime(2026, 4, 1),
                Status = DocumentStatus.Draft
            };
            var cage = new Cage
            {
                CageCode = "CAGE-RESET-001",
                CageName = "Reset Demo Cage"
            };

            db.Projects.Add(project);
            db.Cages.Add(cage);
            db.ProjectCages.Add(new ProjectCage
            {
                Project = project,
                Cage = cage,
                AssignedDate = new DateTime(2026, 4, 1)
            });
            await db.SaveChangesAsync();
        }

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(client, "/api/aqua/OpeningImport/preview", new OpeningImportPreviewRequestDto
        {
            FileName = "reset-opening-import.xlsx",
            SourceSystem = "reset-test",
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
                            ["projectCode"] = "PRJ-RESET-001",
                            ["projectName"] = "Reset Project",
                            ["startDate"] = "2026-04-05",
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
                            ["projectCode"] = "PRJ-RESET-001",
                            ["cageCode"] = "CAGE-RESET-001",
                            ["cageName"] = "Reset Cage",
                        }
                    ]
                }
            ]
        });

        Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
        Assert.NotNull(preview.Data);
        Assert.Equal("Failed", preview.Data!.Status);

        var reset = await PostAsync<OpeningImportResetExistingDataResultDto>(client, $"/api/aqua/OpeningImport/{preview.Data.JobId}/reset-existing-data", new { });
        Assert.True(reset.Success, $"{reset.Message} | {reset.ExceptionMessage}");
        Assert.NotNull(reset.Data);
        Assert.Equal(1, reset.Data!.DeletedProjects);
        Assert.Equal(1, reset.Data.DeletedCages);
        Assert.Equal(1, reset.Data.DeletedProjectCages);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        Assert.False(await verifyDb.Projects.IgnoreQueryFilters().AnyAsync(x => x.ProjectCode == "PRJ-RESET-001"));
        Assert.False(await verifyDb.Cages.IgnoreQueryFilters().AnyAsync(x => x.CageCode == "CAGE-RESET-001"));
        Assert.False(await verifyDb.ProjectCages.IgnoreQueryFilters().AnyAsync(x => x.Project!.ProjectCode == "PRJ-RESET-001" || x.Cage!.CageCode == "CAGE-RESET-001"));
    }

    [Fact]
    public async Task OpeningImport_AcceptsTurkishDateAndExcelSerialDateFormats()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(client, "/api/aqua/OpeningImport/preview", new OpeningImportPreviewRequestDto
        {
            FileName = "localized-opening-import.xlsx",
            SourceSystem = "localized-date-test",
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
                            ["projectCode"] = "PRJ-DATE-TR-001",
                            ["projectName"] = "Localized Date Project",
                            ["startDate"] = "17.06.2022",
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
                        new OpeningImportFieldMappingDto { SourceColumn = "assignedDate", TargetField = "assignedDate" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = "PRJ-DATE-TR-001",
                            ["cageCode"] = "CAGE-DATE-TR-001",
                            ["cageName"] = "Localized Date Cage",
                            ["assignedDate"] = "44729",
                        }
                    ]
                }
            ]
        });

        Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
        Assert.NotNull(preview.Data);
        Assert.Equal("Previewed", preview.Data!.Status);
        Assert.All(preview.Data.Rows, row => Assert.Empty(row.Messages));

        var commit = await PostAsync<OpeningImportCommitResultDto>(client, $"/api/aqua/OpeningImport/{preview.Data.JobId}/commit", new { });
        Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var project = await db.Projects.SingleAsync(x => !x.IsDeleted && x.ProjectCode == "PRJ-DATE-TR-001");
        var projectCage = await db.ProjectCages.Include(x => x.Cage).SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id);

        Assert.Equal(new DateTime(2022, 6, 17), project.StartDate.Date);
        Assert.Equal(new DateTime(2022, 6, 17), projectCage.AssignedDate.Date);
    }

    [Fact]
    public async Task WeatherSeverityDefinition_IsIndependentFromWeatherType()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var code = $"SEV-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        var created = await PostAsync<WeatherSeverityDto>(client, "/api/aqua/WeatherSeverity", new CreateWeatherSeverityDto
        {
            Code = code,
            Name = "Independent Severity",
            Score = 35,
        });

        Assert.True(created.Success, $"{created.Message} | {created.ExceptionMessage}");
        Assert.NotNull(created.Data);
        Assert.Equal(code, created.Data!.Code);
        Assert.Equal(35, created.Data.Score);
    }

    [Fact]
    public async Task OpeningYesterday_FeedingAndMortalityToday_AppearAsSeparateDailyRowsInDashboardReport()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var projectCode = $"PRJ-DAY-{suffix}";
        var cageCode = $"CAGE-DAY-{suffix}";
        var batchCode = $"BATCH-DAY-{suffix}";
        var yesterday = DateTime.Today.AddDays(-1);
        var today = DateTime.Today;

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(client, "/api/aqua/OpeningImport/preview", new OpeningImportPreviewRequestDto
        {
            FileName = "daily-row-split-opening.xlsx",
            SourceSystem = "daily-row-split-test",
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
                            ["projectName"] = "Daily Row Split Project",
                            ["startDate"] = yesterday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
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
                        new OpeningImportFieldMappingDto { SourceColumn = "assignedDate", TargetField = "assignedDate" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode,
                            ["cageCode"] = cageCode,
                            ["cageName"] = "Daily Row Split Cage",
                            ["assignedDate"] = yesterday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
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
                            ["fishCount"] = "1000",
                            ["averageGram"] = "5",
                            ["asOfDate"] = yesterday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        }
                    ]
                }
            ]
        });

        Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
        Assert.NotNull(preview.Data);
        Assert.Equal("Previewed", preview.Data!.Status);

        var commit = await PostAsync<OpeningImportCommitResultDto>(client, $"/api/aqua/OpeningImport/{preview.Data.JobId}/commit", new { });
        Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");

        long projectId;
        long projectCageId;
        long fishBatchId;
        long feedStockId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var project = await db.Projects.SingleAsync(x => !x.IsDeleted && x.ProjectCode == projectCode);
            var projectCage = await db.ProjectCages.SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id);
            var fishBatch = await db.FishBatches.SingleAsync(x => !x.IsDeleted && x.BatchCode == batchCode);
            var feedStock = await db.Stocks.SingleAsync(x => !x.IsDeleted && x.ErpStockCode == "YEM-STD");

            projectId = project.Id;
            projectCageId = projectCage.Id;
            fishBatchId = fishBatch.Id;
            feedStockId = feedStock.Id;
        }

        var feeding = await PostAsync<FeedingLineDto>(client, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            FeedingDate = today,
            FeedingSlot = FeedingSlot.Morning,
            StockId = feedStockId,
            QtyUnit = 2m,
            GramPerUnit = 1000m,
            TotalGram = 2_000m,
        });
        Assert.True(feeding.Success, $"{feeding.Message} | {feeding.ExceptionMessage}");

        var feedingDistribution = await PostAsync<FeedingDistributionDto>(client, "/api/aqua/FeedingDistribution", new CreateFeedingDistributionDto
        {
            FeedingLineId = feeding.Data!.Id,
            FishBatchId = fishBatchId,
            ProjectCageId = projectCageId,
            FeedGram = 2_000m,
        });
        Assert.True(feedingDistribution.Success, $"{feedingDistribution.Message} | {feedingDistribution.ExceptionMessage}");

        var mortality = await PostAsync<MortalityLineDto>(client, "/api/aqua/MortalityLine/auto-header", new CreateMortalityLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            MortalityDate = today,
            FishBatchId = fishBatchId,
            ProjectCageId = projectCageId,
            DeadCount = 10,
        });
        Assert.True(mortality.Success, $"{mortality.Message} | {mortality.ExceptionMessage}");

        var detail = await GetAsync<DashboardProjectDetailDto>(client, $"/api/aqua/dashboard-project/detail/{projectId}");
        Assert.True(detail.Success, $"{detail.Message} | {detail.ExceptionMessage}");
        var cage = Assert.Single(detail.Data!.Cages);
        var yesterdayRow = Assert.Single(cage.DailyRows, x => x.Date == yesterday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        var todayRow = Assert.Single(cage.DailyRows, x => x.Date == today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        Assert.Equal(1000, yesterdayRow.CountDelta);
        Assert.Equal(0m, yesterdayRow.FeedGram);
        Assert.Equal(0, yesterdayRow.DeadCount);
        Assert.Equal(2_000m, todayRow.FeedGram);
        Assert.Equal(10, todayRow.DeadCount);
        Assert.Equal(-10, todayRow.CountDelta);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var movements = await verifyDb.BatchMovements
            .Where(x => !x.IsDeleted && x.FishBatchId == fishBatchId)
            .ToListAsync();
        var movementsByDate = movements
            .GroupBy(x => x.MovementDate.Date)
            .Select(x => new { Date = x.Key, Types = x.Select(m => m.MovementType).ToList() })
            .ToList();

        Assert.Contains(movementsByDate, x => x.Date == yesterday.Date && x.Types.Contains(BatchMovementType.OpeningImport));
        Assert.Contains(movementsByDate, x => x.Date == today.Date && x.Types.Contains(BatchMovementType.Feeding) && x.Types.Contains(BatchMovementType.Mortality));
    }

    [Fact]
    public async Task OpeningImport_GoodsReceiptRowsForOneBatch_CreateOneLineWithCageDistributions()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(
            client,
            "/api/aqua/OpeningImport/preview",
            BuildOpeningGoodsReceiptRequest("PRJ-REC-ONE", "OPEN-REC-ONE", string.Empty, secondReceiptDate: string.Empty));

        Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
        Assert.Equal("Previewed", preview.Data!.Status);
        Assert.Equal(
            "OPEN-REC-ONE",
            preview.Data.Rows.Single(x => x.SheetName == "OpeningGoodsReceipts" && x.RowNumber == 3).NormalizedData["receiptNo"]);

        var commit = await PostAsync<OpeningImportCommitResultDto>(client, $"/api/aqua/OpeningImport/{preview.Data.JobId}/commit", new { });
        Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");
        Assert.Equal(1, commit.Data!.CreatedGoodsReceipts);
        Assert.Equal(1, commit.Data.CreatedGoodsReceiptLines);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var projectId = await db.Projects.Where(x => x.ProjectCode == "PRJ-REC-ONE").Select(x => x.Id).SingleAsync();
        var headers = await db.GoodsReceipts
            .Include(x => x.Lines)
            .ThenInclude(x => x.FishDistributions)
            .Where(x => x.ProjectId == projectId)
            .ToListAsync();

        Assert.Single(headers);
        Assert.Single(headers[0].Lines);
        Assert.Equal(1100, headers[0].Lines.Single().FishCount);
        Assert.Equal(2, headers[0].Lines.Single().FishDistributions.Count);
        Assert.Equal(1100, headers[0].Lines.Single().FishDistributions.Sum(x => x.FishCount));
    }

    [Fact]
    public async Task OpeningImport_ResetExistingDataHardDeletesAlreadyAppliedImportScope()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(
            client,
            "/api/aqua/OpeningImport/preview",
            BuildOpeningGoodsReceiptRequest("PRJ-RESET-APPLIED", "OPEN-RESET-APPLIED", string.Empty, secondReceiptDate: string.Empty));

        Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");

        var commit = await PostAsync<OpeningImportCommitResultDto>(client, $"/api/aqua/OpeningImport/{preview.Data!.JobId}/commit", new { });
        Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");

        var reset = await PostAsync<OpeningImportResetExistingDataResultDto>(client, $"/api/aqua/OpeningImport/{preview.Data.JobId}/reset-existing-data", new { });
        Assert.True(reset.Success, $"{reset.Message} | {reset.ExceptionMessage}");
        Assert.Equal(1, reset.Data!.DeletedProjects);
        Assert.Equal(2, reset.Data.DeletedCages);
        Assert.Equal(1, reset.Data.DeletedGoodsReceipts);
        Assert.Equal(1, reset.Data.DeletedFishBatches);
        Assert.True(reset.Data.DeletedOperationalRecords > 0);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
        Assert.False(await db.Projects.IgnoreQueryFilters().AnyAsync(x => x.ProjectCode == "PRJ-RESET-APPLIED"));
        Assert.False(await db.GoodsReceipts.IgnoreQueryFilters().AnyAsync(x => x.ReceiptNo == "OPEN-RESET-APPLIED"));
    }

    [Fact]
    public async Task OpeningImport_GoodsReceiptRowsForOneProject_BlockConflictingHeaderInformation()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(
            client,
            "/api/aqua/OpeningImport/preview",
            BuildOpeningGoodsReceiptRequest("PRJ-REC-CONFLICT", "OPEN-REC-A", "OPEN-REC-B"));

        Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
        Assert.Equal("Failed", preview.Data!.Status);
        Assert.Contains(
            preview.Data.Rows.Where(x => x.SheetName == "OpeningGoodsReceipts"),
            row => row.Messages.Any(message => message.Contains("fiş no, tarih ve depo", StringComparison.OrdinalIgnoreCase)));

        var commit = await PostAsync<OpeningImportCommitResultDto>(client, $"/api/aqua/OpeningImport/{preview.Data.JobId}/commit", new { });
        Assert.False(commit.Success);
        Assert.Contains("fiş no, tarih ve depo", commit.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OpeningImport_GoodsReceiptRowsForOneBatch_BlockDifferentAverageGram()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");
        var request = BuildOpeningGoodsReceiptRequest("PRJ-REC-BATCH-CONFLICT", "OPEN-REC-BATCH", "OPEN-REC-BATCH");
        request.Sheets.Single(x => x.SheetName == "OpeningGoodsReceipts").Rows[1]["averageGram"] = "6";

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(
            client,
            "/api/aqua/OpeningImport/preview",
            request);

        Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
        Assert.Equal("Failed", preview.Data!.Status);
        Assert.Contains(
            preview.Data.Rows.Where(x => x.SheetName == "OpeningGoodsReceipts"),
            row => row.Messages.Any(message => message.Contains("proje/batch bazında", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task OpeningImport_GoodsReceiptRowsForDifferentBatches_CreateSeparateLines()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");
        var request = BuildOpeningGoodsReceiptRequest("PRJ-REC-TWO-BATCH", "OPEN-REC-TWO-BATCH", "OPEN-REC-TWO-BATCH");
        request.Sheets.Single(x => x.SheetName == "OpeningGoodsReceipts").Rows[1]["batchCode"] = "PRJ-REC-TWO-BATCH-B2";

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(
            client,
            "/api/aqua/OpeningImport/preview",
            request);

        Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
        Assert.Equal("Previewed", preview.Data!.Status);

        var commit = await PostAsync<OpeningImportCommitResultDto>(client, $"/api/aqua/OpeningImport/{preview.Data.JobId}/commit", new { });
        Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");
        Assert.Equal(2, commit.Data!.CreatedGoodsReceiptLines);
    }

    [Fact]
    public async Task HttpLifecycle_OpeningToShipment_KeepsEndpointsAndReportsAligned()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var preview = await PostAsync<OpeningImportPreviewResponseDto>(client, "/api/aqua/OpeningImport/preview", new OpeningImportPreviewRequestDto
        {
            FileName = "http-lifecycle.xlsx",
            SourceSystem = "http-integration-test",
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
                            ["projectCode"] = "PRJ-HTTP-001",
                            ["projectName"] = "HTTP Integration Farm",
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
                            ["projectCode"] = "PRJ-HTTP-001",
                            ["cageCode"] = "CAGE-HTTP-01",
                            ["cageName"] = "HTTP Main Cage",
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
                            ["projectCode"] = "PRJ-HTTP-001",
                            ["cageCode"] = "CAGE-HTTP-01",
                            ["batchCode"] = "BATCH-HTTP-PLAMUT-5G",
                            ["fishStockCode"] = "PLAMUT-5G",
                            ["fishCount"] = "10000",
                            ["averageGram"] = "5",
                            ["asOfDate"] = "2026-04-01",
                        }
                    ]
                }
            ]
        });

        Assert.True(preview.Success, $"{preview.Message} | {preview.ExceptionMessage}");
        Assert.NotNull(preview.Data);

        var commit = await PostAsync<OpeningImportCommitResultDto>(client, $"/api/aqua/OpeningImport/{preview.Data!.JobId}/commit", new { });
        Assert.True(commit.Success, $"{commit.Message} | {commit.ExceptionMessage}");

        long projectId;
        long projectCageId;
        long openingBatchId;
        long fish10StockId;
        long feedStockId;
        long warehouseId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var project = await db.Projects.SingleAsync(x => !x.IsDeleted && x.ProjectCode == "PRJ-HTTP-001");
            var projectCage = await db.ProjectCages.Include(x => x.Cage).SingleAsync(x => !x.IsDeleted && x.ProjectId == project.Id);
            var openingBatch = await db.FishBatches.SingleAsync(x => !x.IsDeleted && x.BatchCode == "BATCH-HTTP-PLAMUT-5G");
            var fish10Stock = await db.Stocks.SingleAsync(x => !x.IsDeleted && x.ErpStockCode == "PLAMUT-10G");
            var feedStock = await db.Stocks.SingleAsync(x => !x.IsDeleted && x.ErpStockCode == "YEM-STD");
            var warehouse = await db.Warehouses.SingleAsync(x => !x.IsDeleted && x.ErpWarehouseCode == 10);

            projectId = project.Id;
            projectCageId = projectCage.Id;
            openingBatchId = openingBatch.Id;
            fish10StockId = fish10Stock.Id;
            feedStockId = feedStock.Id;
            warehouseId = warehouse.Id;
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            await AquaHttpTestWebApplicationFactory.SeedFeedPurchaseHistoryAsync(db, projectId, warehouseId, feedStockId);
        }

        var convertedBatch = await PostAsync<FishBatchDto>(client, "/api/aqua/FishBatch", new CreateFishBatchDto
        {
            ProjectId = projectId,
            BatchCode = "BATCH-HTTP-PLAMUT-10G",
            FishStockId = fish10StockId,
            CurrentAverageGram = 10m,
            StartDate = new DateTime(2026, 4, 3),
            TargetHarvestAverageGram = 20m,
        });
        Assert.True(convertedBatch.Success, $"{convertedBatch.Message} | {convertedBatch.ExceptionMessage}");
        var convertedBatchId = convertedBatch.Data!.Id;

        var feedingDay2 = await PostAsync<FeedingLineDto>(client, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            FeedingDate = new DateTime(2026, 4, 2),
            FeedingSlot = FeedingSlot.Morning,
            StockId = feedStockId,
            QtyUnit = 20m,
            GramPerUnit = 1000m,
            TotalGram = 20_000m,
        });
        Assert.True(feedingDay2.Success, $"{feedingDay2.Message} | {feedingDay2.ExceptionMessage}");
        Assert.True((await PostAsync<FeedingDistributionDto>(client, "/api/aqua/FeedingDistribution", new CreateFeedingDistributionDto
        {
            FeedingLineId = feedingDay2.Data!.Id,
            FishBatchId = openingBatchId,
            ProjectCageId = projectCageId,
            FeedGram = 20_000m,
        })).Success);

        var mortalityDay2 = await PostAsync<MortalityLineDto>(client, "/api/aqua/MortalityLine/auto-header", new CreateMortalityLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            MortalityDate = new DateTime(2026, 4, 2),
            FishBatchId = openingBatchId,
            ProjectCageId = projectCageId,
            DeadCount = 100,
        });
        Assert.True(mortalityDay2.Success, $"{mortalityDay2.Message} | {mortalityDay2.ExceptionMessage}");

        var feedingDay3 = await PostAsync<FeedingLineDto>(client, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            FeedingDate = new DateTime(2026, 4, 3),
            FeedingSlot = FeedingSlot.Morning,
            StockId = feedStockId,
            QtyUnit = 25m,
            GramPerUnit = 1000m,
            TotalGram = 25_000m,
        });
        Assert.True(feedingDay3.Success);
        Assert.True((await PostAsync<FeedingDistributionDto>(client, "/api/aqua/FeedingDistribution", new CreateFeedingDistributionDto
        {
            FeedingLineId = feedingDay3.Data!.Id,
            FishBatchId = openingBatchId,
            ProjectCageId = projectCageId,
            FeedGram = 25_000m,
        })).Success);

        var stockConvert = await PostAsync<StockConvertLineDto>(client, "/api/aqua/StockConvertLine/auto-header", new CreateStockConvertLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            ConvertDate = new DateTime(2026, 4, 3),
            FromFishBatchId = openingBatchId,
            ToFishBatchId = convertedBatchId,
            FromProjectCageId = projectCageId,
            ToProjectCageId = projectCageId,
            FishCount = 4_000,
            AverageGram = 5m,
            NewAverageGram = 5m,
            BiomassGram = 20_000m,
        });
        Assert.True(stockConvert.Success, $"{stockConvert.Message} | {stockConvert.ExceptionMessage}");

        var feedingDay4 = await PostAsync<FeedingLineDto>(client, "/api/aqua/FeedingLine/auto-header", new CreateFeedingLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            FeedingDate = new DateTime(2026, 4, 4),
            FeedingSlot = FeedingSlot.Morning,
            StockId = feedStockId,
            QtyUnit = 18m,
            GramPerUnit = 1000m,
            TotalGram = 18_000m,
        });
        Assert.True(feedingDay4.Success);
        Assert.True((await PostAsync<FeedingDistributionDto>(client, "/api/aqua/FeedingDistribution", new CreateFeedingDistributionDto
        {
            FeedingLineId = feedingDay4.Data!.Id,
            FishBatchId = openingBatchId,
            ProjectCageId = projectCageId,
            FeedGram = 10_000m,
        })).Success);
        Assert.True((await PostAsync<FeedingDistributionDto>(client, "/api/aqua/FeedingDistribution", new CreateFeedingDistributionDto
        {
            FeedingLineId = feedingDay4.Data!.Id,
            FishBatchId = convertedBatchId,
            ProjectCageId = projectCageId,
            FeedGram = 8_000m,
        })).Success);

        var cageWarehouseLine = await PostAsync<CageWarehouseTransferLineDto>(client, "/api/aqua/CageWarehouseTransferLine/auto-header", new CreateCageWarehouseTransferLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            TransferDate = new DateTime(2026, 4, 4),
            FishBatchId = convertedBatchId,
            FromProjectCageId = projectCageId,
            ToWarehouseId = warehouseId,
            FishCount = 1_500,
            AverageGram = 10m,
            BiomassGram = 15_000m,
        });
        Assert.True(cageWarehouseLine.Success);

        long cageWarehouseHeaderId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            cageWarehouseHeaderId = await db.CageWarehouseTransfers
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.TransferDate.Date == new DateTime(2026, 4, 4))
                .Select(x => x.Id)
                .SingleAsync();
        }
        Assert.True((await PostAsync<bool>(client, $"/api/aqua/posting/cage-warehouse-transfer/{cageWarehouseHeaderId}", new { })).Success);

        var warehouseCageLine = await PostAsync<WarehouseCageTransferLineDto>(client, "/api/aqua/WarehouseCageTransferLine/auto-header", new CreateWarehouseCageTransferLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            TransferDate = new DateTime(2026, 4, 4),
            FishBatchId = convertedBatchId,
            FromWarehouseId = warehouseId,
            ToProjectCageId = projectCageId,
            FishCount = 500,
            AverageGram = 10m,
            BiomassGram = 5_000m,
        });
        Assert.True(warehouseCageLine.Success);

        long warehouseCageHeaderId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            warehouseCageHeaderId = await db.WarehouseCageTransfers
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.TransferDate.Date == new DateTime(2026, 4, 4))
                .Select(x => x.Id)
                .SingleAsync();
        }
        Assert.True((await PostAsync<bool>(client, $"/api/aqua/posting/warehouse-cage-transfer/{warehouseCageHeaderId}", new { })).Success);

        Assert.True((await PostAsync<ShipmentLineDto>(client, "/api/aqua/ShipmentLine/auto-header", new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            ShipmentDate = new DateTime(2026, 4, 4),
            FishBatchId = convertedBatchId,
            FromProjectCageId = projectCageId,
            FishCount = 1_000,
            AverageGram = 10m,
            BiomassGram = 10_000m,
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 210m,
        })).Success);

        Assert.True((await PostAsync<ShipmentLineDto>(client, "/api/aqua/ShipmentLine/auto-header", new CreateShipmentLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            ShipmentDate = new DateTime(2026, 4, 4),
            FishBatchId = convertedBatchId,
            FromProjectCageId = projectCageId,
            FishCount = 200,
            AverageGram = 10m,
            BiomassGram = 2_000m,
            CurrencyCode = "TRY",
            ExchangeRate = 1m,
            UnitPrice = 230m,
        })).Success);

        long shipmentHeaderId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            shipmentHeaderId = await db.Shipments
                .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.ShipmentDate.Date == new DateTime(2026, 4, 4))
                .Select(x => x.Id)
                .SingleAsync();
        }
        Assert.True((await PostAsync<bool>(client, $"/api/aqua/posting/shipment/{shipmentHeaderId}", new { })).Success);

        var mortalityDay4 = await PostAsync<MortalityLineDto>(client, "/api/aqua/MortalityLine/auto-header", new CreateMortalityLineWithAutoHeaderDto
        {
            ProjectId = projectId,
            MortalityDate = new DateTime(2026, 4, 4),
            FishBatchId = openingBatchId,
            ProjectCageId = projectCageId,
            DeadCount = 50,
        });
        Assert.True(mortalityDay4.Success);

        var snapshot = await PostAsync<List<ProjectCageDailyKpiSnapshotDto>>(client, "/api/aqua/ProjectCageDailyKpi/snapshot", new CreateProjectCageDailyKpiSnapshotRequest
        {
            ProjectId = projectId,
            SnapshotDate = new DateTime(2026, 4, 4),
        });
        Assert.True(snapshot.Success, $"{snapshot.Message} | {snapshot.ExceptionMessage}");
        Assert.Equal(2, snapshot.Data!.Count);

        var latestKpis = await GetAsync<List<ProjectCageDailyKpiSnapshotDto>>(client, $"/api/aqua/ProjectCageDailyKpi?projectId={projectId}&snapshotDate=2026-04-04");
        Assert.True(latestKpis.Success);
        Assert.Equal(2, latestKpis.Data!.Count);
        Assert.Equal(7_650, latestKpis.Data.Sum(x => x.LiveCount));
        Assert.Equal(47.25m, latestKpis.Data.Sum(x => x.BiomassKg));
        Assert.Equal(63m, latestKpis.Data.Sum(x => x.FeedKgPeriod));

        var devirFcr = await PostAsync<DevirFcrReportDto>(client, "/api/kpi-report/devir-fcr", new DevirFcrReportRequestDto
        {
            ProjectIds = [projectId]
        });
        Assert.True(devirFcr.Success, $"{devirFcr.Message} | {devirFcr.ExceptionMessage}");
        var devirFcrRow = Assert.Single(devirFcr.Data!.Rows);
        Assert.Equal(50m, devirFcrRow.OpeningBiomassKg);
        Assert.Equal(57.25m, devirFcrRow.EndingBiomassKg);
        Assert.Equal(12m, devirFcrRow.ShippedBiomassKg);
        Assert.Equal(0.75m, devirFcrRow.MortalityBiomassKg);
        Assert.Equal(70m, devirFcrRow.ProducedBiomassKg);
        Assert.Equal(63m, devirFcrRow.TotalFeedKg);
        Assert.Equal(0.9m, devirFcrRow.Fcr);

        var preLifecycleDevirFcr = await PostAsync<DevirFcrReportDto>(client, "/api/kpi-report/devir-fcr", new DevirFcrReportRequestDto
        {
            ProjectIds = [projectId],
            FromDate = new DateTime(2024, 1, 1),
            ToDate = new DateTime(2026, 4, 4),
        });
        Assert.True(preLifecycleDevirFcr.Success, $"{preLifecycleDevirFcr.Message} | {preLifecycleDevirFcr.ExceptionMessage}");
        var preLifecycleDevirFcrRow = Assert.Single(preLifecycleDevirFcr.Data!.Rows);
        Assert.Equal(10_000, preLifecycleDevirFcrRow.OpeningFishCount);
        Assert.Equal(150, preLifecycleDevirFcrRow.MortalityFishCount);
        Assert.Equal(1.5m, preLifecycleDevirFcrRow.MortalityPct);

        var rangedDevirFcr = await PostAsync<DevirFcrReportDto>(client, "/api/kpi-report/devir-fcr", new DevirFcrReportRequestDto
        {
            ProjectIds = [projectId],
            FromDate = new DateTime(2026, 4, 3),
            ToDate = new DateTime(2026, 4, 4),
        });
        Assert.True(rangedDevirFcr.Success, $"{rangedDevirFcr.Message} | {rangedDevirFcr.ExceptionMessage}");
        Assert.Equal(new DateTime(2026, 4, 3), rangedDevirFcr.Data!.FromDate);
        Assert.Equal(new DateTime(2026, 4, 4), rangedDevirFcr.Data.ToDate);
        var rangedDevirFcrRow = Assert.Single(rangedDevirFcr.Data.Rows);
        Assert.Equal(9_900, rangedDevirFcrRow.OpeningFishCount);
        Assert.Equal(1_200, rangedDevirFcrRow.ShipmentFishCount);
        Assert.Equal(50, rangedDevirFcrRow.MortalityFishCount);
        Assert.Equal(43m, rangedDevirFcrRow.TotalFeedKg);
        Assert.Equal(12m, rangedDevirFcrRow.ShippedBiomassKg);
        Assert.Equal(0.25m, rangedDevirFcrRow.MortalityBiomassKg);

        var invalidDevirFcrRange = await PostAsync<DevirFcrReportDto>(client, "/api/kpi-report/devir-fcr", new DevirFcrReportRequestDto
        {
            ProjectIds = [projectId],
            FromDate = new DateTime(2026, 4, 4),
            ToDate = new DateTime(2026, 4, 3),
        });
        Assert.False(invalidDevirFcrRange.Success);

        var dashboardSummary = await PostAsync<DashboardProjectsResponseDto>(client, "/api/aqua/dashboard-project/summary", new DashboardProjectsRequestDto
        {
            ProjectIds = [projectId]
        });
        Assert.True(dashboardSummary.Success, $"{dashboardSummary.Message} | {dashboardSummary.ExceptionMessage}");
        var dashboardProject = Assert.Single(dashboardSummary.Data!.Projects);
        Assert.Equal(devirFcrRow.EndingBiomassKg, dashboardProject.TotalSystemBiomassGram / 1000m);
        Assert.Equal(47.25m, dashboardProject.CageBiomassGram / 1000m);
        Assert.Equal(10m, dashboardProject.WarehouseBiomassGram / 1000m);
        Assert.Equal(devirFcrRow.ShippedBiomassKg, dashboardProject.TotalShipmentBiomassGram / 1000m);
        Assert.Equal(devirFcrRow.MortalityBiomassKg, dashboardProject.TotalDeadBiomassGram / 1000m);
        Assert.Equal(devirFcrRow.Fcr, dashboardProject.Fcr);

        var dashboardDetail = await GetAsync<DashboardProjectDetailDto>(client, $"/api/aqua/dashboard-project/detail/{projectId}");
        Assert.True(dashboardDetail.Success, $"{dashboardDetail.Message} | {dashboardDetail.ExceptionMessage}");
        var dashboardCage = Assert.Single(dashboardDetail.Data!.Cages);
        Assert.Equal(devirFcrRow.TotalFeedKg, dashboardCage.TotalFeedGram / 1000m);
        Assert.Equal(devirFcrRow.ShippedBiomassKg, dashboardCage.TotalShipmentBiomassGram / 1000m);
        Assert.Equal(devirFcrRow.MortalityBiomassKg, dashboardCage.TotalDeadBiomassGram / 1000m);
        Assert.Equal(1.05m, dashboardCage.Fcr);
        Assert.Equal(4, dashboardCage.DailyRows.Count);

        var day1 = Assert.Single(dashboardCage.DailyRows, x => x.Date == "2026-04-01");
        Assert.Equal(10_000, day1.CountDelta);
        Assert.Equal(50m, day1.BiomassDelta / 1000m);
        Assert.Equal(0m, day1.FeedGram);
        Assert.False(day1.Fed);

        var day2 = Assert.Single(dashboardCage.DailyRows, x => x.Date == "2026-04-02");
        Assert.Equal(20m, day2.FeedGram / 1000m);
        Assert.Equal(100, day2.DeadCount);
        Assert.Equal(0.5m, day2.DeadBiomassGram / 1000m);
        Assert.Equal(0, day2.ShipmentFishCount);
        Assert.True(day2.Fed);

        var day3 = Assert.Single(dashboardCage.DailyRows, x => x.Date == "2026-04-03");
        Assert.Equal(25m, day3.FeedGram / 1000m);
        Assert.Equal(1, day3.StockConvertCount);
        Assert.Equal(20m, day3.BiomassDelta / 1000m);
        Assert.Equal(0, day3.DeadCount);
        Assert.True(day3.Fed);

        var day4 = Assert.Single(dashboardCage.DailyRows, x => x.Date == "2026-04-04");
        Assert.Equal(18m, day4.FeedGram / 1000m);
        Assert.Equal(50, day4.DeadCount);
        Assert.Equal(0.25m, day4.DeadBiomassGram / 1000m);
        Assert.Equal(2, day4.ShipmentCount);
        Assert.Equal(1_200, day4.ShipmentFishCount);
        Assert.Equal(12m, day4.ShipmentBiomassGram / 1000m);
        Assert.True(day4.Fed);

        var cageBalances = await GetAsync<PagedResponse<BatchCageBalanceDto>>(client, "/api/aqua/BatchCageBalance");
        Assert.True(cageBalances.Success);
        Assert.True(cageBalances.Data!.Items.Count >= 2);

        var movements = await GetAsync<PagedResponse<BatchMovementDto>>(client, "/api/aqua/BatchMovement");
        Assert.True(movements.Success);
        Assert.True(movements.Data!.TotalCount >= 15);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var openingBalance = await db.BatchCageBalances.SingleAsync(x => !x.IsDeleted && x.FishBatchId == openingBatchId && x.ProjectCageId == projectCageId);
            var convertedBalance = await db.BatchCageBalances.SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.ProjectCageId == projectCageId);
            var warehouseBalance = await db.BatchWarehouseBalances.SingleAsync(x => !x.IsDeleted && x.FishBatchId == convertedBatchId && x.WarehouseId == warehouseId);
            var shipmentLines = await db.ShipmentLines
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Id)
                .ToListAsync();

            Assert.Equal(5_850, openingBalance.LiveCount);
            Assert.Equal(29_250m, openingBalance.BiomassGram);
            Assert.Equal(1_800, convertedBalance.LiveCount);
            Assert.Equal(18_000m, convertedBalance.BiomassGram);
            Assert.Equal(1_000, warehouseBalance.LiveCount);
            Assert.Equal(10_000m, warehouseBalance.BiomassGram);

            var weightedFeedCostPerKg = CalculateWeightedFeedCostPerKg(await db.GoodsReceiptLines.Where(x => !x.IsDeleted && x.StockId == feedStockId).ToListAsync());
            var weightedSalePricePerKg = CalculateWeightedSalePricePerKg(shipmentLines);
            Assert.Equal(61.079m, weightedFeedCostPerKg);
            Assert.Equal(213.333m, weightedSalePricePerKg);
        }
    }

    [Fact]
    public async Task DevirFcrReport_CustomerLikeSingleProject_MatchesExcelOutputBiomassFcrFormula()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var projectCode = $"FCR-CUST-{suffix}";
        const decimal openingBiomassGram = 10_000_000m;
        const decimal growthBiomassGram = 40_000_000m;
        const decimal shipmentBiomassGram = 15_000_000m;
        const decimal mortalityBiomassGram = 5_000_000m;
        const decimal feedGram = 100_000_000m;

        long projectId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var fishStockId = await db.Stocks
                .Where(x => !x.IsDeleted && x.ErpStockCode == "PLAMUT-5G")
                .Select(x => x.Id)
                .SingleAsync();
            var feedStockId = await db.Stocks
                .Where(x => !x.IsDeleted && x.ErpStockCode == "YEM-STD")
                .Select(x => x.Id)
                .SingleAsync();

            var project = new Project
            {
                ProjectCode = projectCode,
                ProjectName = "Customer-like FCR Project",
                StartDate = new DateTime(2026, 4, 1),
                Status = DocumentStatus.Posted,
            };
            var cage = new Cage
            {
                CageCode = $"FCR-CAGE-{suffix}",
                CageName = "Customer-like FCR Cage",
            };

            db.Projects.Add(project);
            db.Cages.Add(cage);
            await db.SaveChangesAsync();

            var projectCage = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cage.Id,
                AssignedDate = project.StartDate,
            };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                BatchCode = $"FCR-BATCH-{suffix}",
                FishStockId = fishStockId,
                CurrentAverageGram = 500m,
                StartDate = project.StartDate,
                TargetHarvestAverageGram = 600m,
            };

            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            var feeding = new Feeding
            {
                ProjectId = project.Id,
                FeedingNo = $"FCR-FEED-{suffix}",
                FeedingDate = new DateTime(2026, 4, 15),
                FeedingSlot = FeedingSlot.Morning,
                SourceType = FeedingSourceType.Manual,
                Status = DocumentStatus.Posted,
            };
            db.Feedings.Add(feeding);
            await db.SaveChangesAsync();

            var feedingLine = new FeedingLine
            {
                FeedingId = feeding.Id,
                StockId = feedStockId,
                QtyUnit = feedGram / 1000m,
                GramPerUnit = 1000m,
                TotalGram = feedGram,
            };
            db.FeedingLines.Add(feedingLine);
            await db.SaveChangesAsync();

            db.FeedingDistributions.Add(new FeedingDistribution
            {
                FeedingLineId = feedingLine.Id,
                FishBatchId = batch.Id,
                ProjectCageId = projectCage.Id,
                FeedGram = feedGram,
            });

            var mortality = new Mortality
            {
                ProjectId = project.Id,
                MortalityDate = new DateTime(2026, 4, 20),
                Status = DocumentStatus.Posted,
            };
            var shipment = new Shipment
            {
                ProjectId = project.Id,
                ShipmentNo = $"FCR-SHIP-{suffix}",
                ShipmentDate = new DateTime(2026, 4, 25),
                Status = DocumentStatus.Posted,
            };
            db.Mortalities.Add(mortality);
            db.Shipments.Add(shipment);
            await db.SaveChangesAsync();

            db.MortalityLines.Add(new MortalityLine
            {
                MortalityId = mortality.Id,
                FishBatchId = batch.Id,
                ProjectCageId = projectCage.Id,
                DeadCount = 10_000,
            });
            db.ShipmentLines.Add(new ShipmentLine
            {
                ShipmentId = shipment.Id,
                FishBatchId = batch.Id,
                FromProjectCageId = projectCage.Id,
                FishCount = 30_000,
                AverageGram = 500m,
                BiomassGram = shipmentBiomassGram,
                CurrencyCode = "TRY",
                ExchangeRate = 1m,
                UnitPrice = 200m,
            });

            db.BatchMovements.AddRange(
                new BatchMovement
                {
                    FishBatchId = batch.Id,
                    ProjectCageId = projectCage.Id,
                    MovementDate = project.StartDate,
                    MovementType = BatchMovementType.OpeningImport,
                    SignedCount = 100_000,
                    SignedBiomassGram = openingBiomassGram,
                    ReferenceTable = "FcrIntegrationTest",
                    ReferenceId = project.Id,
                },
                new BatchMovement
                {
                    FishBatchId = batch.Id,
                    ProjectCageId = projectCage.Id,
                    MovementDate = new DateTime(2026, 4, 10),
                    MovementType = BatchMovementType.Weighing,
                    SignedCount = 0,
                    SignedBiomassGram = growthBiomassGram,
                    ReferenceTable = "FcrIntegrationTest",
                    ReferenceId = project.Id,
                },
                new BatchMovement
                {
                    FishBatchId = batch.Id,
                    ProjectCageId = projectCage.Id,
                    MovementDate = mortality.MortalityDate,
                    MovementType = BatchMovementType.Mortality,
                    SignedCount = -10_000,
                    SignedBiomassGram = -mortalityBiomassGram,
                    ReferenceTable = "FcrIntegrationTest",
                    ReferenceId = mortality.Id,
                },
                new BatchMovement
                {
                    FishBatchId = batch.Id,
                    ProjectCageId = projectCage.Id,
                    MovementDate = shipment.ShipmentDate,
                    MovementType = BatchMovementType.Shipment,
                    SignedCount = -30_000,
                    SignedBiomassGram = -shipmentBiomassGram,
                    ReferenceTable = "FcrIntegrationTest",
                    ReferenceId = shipment.Id,
                });

            await db.SaveChangesAsync();
            projectId = project.Id;
        }

        var devirFcr = await PostAsync<DevirFcrReportDto>(client, "/api/kpi-report/devir-fcr", new DevirFcrReportRequestDto
        {
            ProjectIds = [projectId]
        });
        Assert.True(devirFcr.Success, $"{devirFcr.Message} | {devirFcr.ExceptionMessage}");

        var row = Assert.Single(devirFcr.Data!.Rows);
        Assert.Equal(projectCode, row.ProjectCode);
        Assert.Equal(100_000, row.OpeningFishCount);
        Assert.Equal(30_000, row.ShipmentFishCount);
        Assert.Equal(10_000, row.MortalityFishCount);
        Assert.Equal(10m, row.MortalityPct);
        Assert.Equal(60_000, row.EndingFishCount);
        Assert.Equal(500m, row.EndingAverageGram);
        Assert.Equal(10_000m, row.OpeningBiomassKg);
        Assert.Equal(30_000m, row.EndingBiomassKg);
        Assert.Equal(15_000m, row.ShippedBiomassKg);
        Assert.Equal(5_000m, row.MortalityBiomassKg);
        Assert.Equal(50_000m, row.ProducedBiomassKg);
        Assert.Equal(100_000m, row.TotalFeedKg);
        Assert.Equal(2m, row.Fcr);

        var dashboardSummary = await PostAsync<DashboardProjectsResponseDto>(client, "/api/aqua/dashboard-project/summary", new DashboardProjectsRequestDto
        {
            ProjectIds = [projectId]
        });
        Assert.True(dashboardSummary.Success, $"{dashboardSummary.Message} | {dashboardSummary.ExceptionMessage}");

        var dashboardProject = Assert.Single(dashboardSummary.Data!.Projects);
        Assert.Equal(row.EndingBiomassKg, dashboardProject.TotalSystemBiomassGram / 1000m);
        Assert.Equal(row.ShippedBiomassKg, dashboardProject.TotalShipmentBiomassGram / 1000m);
        Assert.Equal(row.MortalityBiomassKg, dashboardProject.TotalDeadBiomassGram / 1000m);
        Assert.Equal(row.Fcr, dashboardProject.Fcr);
    }

    [Fact]
    public async Task DashboardProjectSummary_UnrepresentedShipment_DoesNotDoubleCountCurrentBiomassForFcr()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var projectCode = $"FCR-IDA-{suffix}";
        const int openingFishCount = 590_000;
        const int mortalityFishCount = 214_865;
        const int shipmentFishCount = 375_135;
        const decimal openingAndShipmentBiomassGram = 188_295_000m;
        const decimal feedGram = 565_093_000m;
        const decimal expectedFcr = 3.001m;

        long projectId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var fishStockId = await db.Stocks
                .Where(x => !x.IsDeleted && x.ErpStockCode == "PLAMUT-5G")
                .Select(x => x.Id)
                .SingleAsync();
            var feedStockId = await db.Stocks
                .Where(x => !x.IsDeleted && x.ErpStockCode == "YEM-STD")
                .Select(x => x.Id)
                .SingleAsync();

            var project = new Project
            {
                ProjectCode = projectCode,
                ProjectName = "IDA-like closed FCR project",
                StartDate = new DateTime(2026, 2, 1),
                Status = DocumentStatus.Posted,
            };
            var cage = new Cage
            {
                CageCode = $"FCR-IDA-CAGE-{suffix}",
                CageName = "IDA-like A2 Cage",
            };

            db.Projects.Add(project);
            db.Cages.Add(cage);
            await db.SaveChangesAsync();

            var projectCage = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cage.Id,
                AssignedDate = project.StartDate,
                ReleasedDate = new DateTime(2026, 2, 28),
            };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                BatchCode = $"FCR-IDA-BATCH-{suffix}",
                FishStockId = fishStockId,
                CurrentAverageGram = openingAndShipmentBiomassGram / openingFishCount,
                StartDate = project.StartDate,
                TargetHarvestAverageGram = openingAndShipmentBiomassGram / shipmentFishCount,
            };

            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();

            var feeding = new Feeding
            {
                ProjectId = project.Id,
                FeedingNo = $"FCR-IDA-FEED-{suffix}",
                FeedingDate = new DateTime(2026, 2, 15),
                FeedingSlot = FeedingSlot.Morning,
                SourceType = FeedingSourceType.Manual,
                Status = DocumentStatus.Posted,
            };
            db.Feedings.Add(feeding);
            await db.SaveChangesAsync();

            var feedingLine = new FeedingLine
            {
                FeedingId = feeding.Id,
                StockId = feedStockId,
                QtyUnit = feedGram / 1000m,
                GramPerUnit = 1000m,
                TotalGram = feedGram,
            };
            db.FeedingLines.Add(feedingLine);
            await db.SaveChangesAsync();

            db.FeedingDistributions.Add(new FeedingDistribution
            {
                FeedingLineId = feedingLine.Id,
                FishBatchId = batch.Id,
                ProjectCageId = projectCage.Id,
                FeedGram = feedGram,
            });

            var mortality = new Mortality
            {
                ProjectId = project.Id,
                MortalityDate = new DateTime(2026, 2, 20),
                Status = DocumentStatus.Posted,
            };
            var shipment = new Shipment
            {
                ProjectId = project.Id,
                ShipmentNo = $"FCR-IDA-SHIP-{suffix}",
                ShipmentDate = new DateTime(2026, 2, 28),
                Status = DocumentStatus.Posted,
            };
            db.Mortalities.Add(mortality);
            db.Shipments.Add(shipment);
            await db.SaveChangesAsync();

            db.MortalityLines.Add(new MortalityLine
            {
                MortalityId = mortality.Id,
                FishBatchId = batch.Id,
                ProjectCageId = projectCage.Id,
                DeadCount = mortalityFishCount,
            });
            db.ShipmentLines.Add(new ShipmentLine
            {
                ShipmentId = shipment.Id,
                FishBatchId = batch.Id,
                FromProjectCageId = projectCage.Id,
                FishCount = shipmentFishCount,
                AverageGram = openingAndShipmentBiomassGram / shipmentFishCount,
                BiomassGram = openingAndShipmentBiomassGram,
                CurrencyCode = "TRY",
                ExchangeRate = 1m,
                UnitPrice = 200m,
            });

            db.BatchCageBalances.Add(new BatchCageBalance
            {
                FishBatchId = batch.Id,
                ProjectCageId = projectCage.Id,
                LiveCount = shipmentFishCount,
                BiomassGram = openingAndShipmentBiomassGram,
                AverageGram = openingAndShipmentBiomassGram / shipmentFishCount,
            });
            db.BatchMovements.AddRange(
                new BatchMovement
                {
                    FishBatchId = batch.Id,
                    ProjectCageId = projectCage.Id,
                    MovementDate = project.StartDate,
                    MovementType = BatchMovementType.OpeningImport,
                    SignedCount = openingFishCount,
                    SignedBiomassGram = openingAndShipmentBiomassGram,
                    ReferenceTable = "FcrIdaLikeIntegrationTest",
                    ReferenceId = project.Id,
                },
                new BatchMovement
                {
                    FishBatchId = batch.Id,
                    ProjectCageId = projectCage.Id,
                    MovementDate = mortality.MortalityDate,
                    MovementType = BatchMovementType.Mortality,
                    SignedCount = -mortalityFishCount,
                    SignedBiomassGram = 0m,
                    ReferenceTable = "FcrIdaLikeIntegrationTest",
                    ReferenceId = mortality.Id,
                });

            await db.SaveChangesAsync();
            projectId = project.Id;
        }

        var devirFcr = await PostAsync<DevirFcrReportDto>(client, "/api/kpi-report/devir-fcr", new DevirFcrReportRequestDto
        {
            ProjectIds = [projectId]
        });
        Assert.True(devirFcr.Success, $"{devirFcr.Message} | {devirFcr.ExceptionMessage}");

        var row = Assert.Single(devirFcr.Data!.Rows);
        Assert.Equal(shipmentFishCount, row.ShipmentFishCount);
        Assert.Equal(mortalityFishCount, row.MortalityFishCount);
        Assert.Equal(0, row.EndingFishCount);
        Assert.Equal(188_295m, row.ProducedBiomassKg);
        Assert.Equal(565_093m, row.TotalFeedKg);
        Assert.Equal(expectedFcr, row.Fcr);

        var dashboardSummary = await PostAsync<DashboardProjectsResponseDto>(client, "/api/aqua/dashboard-project/summary", new DashboardProjectsRequestDto
        {
            ProjectIds = [projectId]
        });
        Assert.True(dashboardSummary.Success, $"{dashboardSummary.Message} | {dashboardSummary.ExceptionMessage}");

        var dashboardProject = Assert.Single(dashboardSummary.Data!.Projects);
        Assert.Equal(0, dashboardProject.CageFish);
        Assert.Equal(0m, dashboardProject.CageBiomassGram);
        Assert.Equal(row.Fcr, dashboardProject.Fcr);

        var dashboardCage = Assert.Single(dashboardProject.Cages);
        Assert.Equal(0, dashboardCage.CurrentFishCount);
        Assert.Equal(0m, dashboardCage.CurrentBiomassGram);
        Assert.Equal(row.Fcr, dashboardCage.Fcr);

        var dashboardDetail = await GetAsync<DashboardProjectDetailDto>(client, $"/api/aqua/dashboard-project/detail/{projectId}");
        Assert.True(dashboardDetail.Success, $"{dashboardDetail.Message} | {dashboardDetail.ExceptionMessage}");

        var detailCage = Assert.Single(dashboardDetail.Data!.Cages);
        Assert.Equal(0, detailCage.CurrentFishCount);
        Assert.Equal(0m, detailCage.CurrentBiomassGram);
        Assert.Equal(row.Fcr, detailCage.Fcr);
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

    private static OpeningImportPreviewRequestDto BuildOpeningGoodsReceiptRequest(
        string projectCode,
        string firstReceiptNo,
        string secondReceiptNo,
        string secondReceiptDate = "2026-04-01")
    {
        return new OpeningImportPreviewRequestDto
        {
            FileName = "opening-goods-receipt-rule.xlsx",
            SourceSystem = "integration-test",
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
                            ["projectName"] = "Receipt Rule Project",
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
                        new Dictionary<string, string?> { ["projectCode"] = projectCode, ["cageCode"] = $"{projectCode}-C1", ["cageName"] = "Cage 1" },
                        new Dictionary<string, string?> { ["projectCode"] = projectCode, ["cageCode"] = $"{projectCode}-C2", ["cageName"] = "Cage 2" },
                    ]
                },
                new OpeningImportSheetPayloadDto
                {
                    SheetName = "OpeningGoodsReceipts",
                    Mappings =
                    [
                        new OpeningImportFieldMappingDto { SourceColumn = "projectCode", TargetField = "projectCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "cageCode", TargetField = "cageCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "receiptNo", TargetField = "receiptNo" },
                        new OpeningImportFieldMappingDto { SourceColumn = "receiptDate", TargetField = "receiptDate" },
                        new OpeningImportFieldMappingDto { SourceColumn = "batchCode", TargetField = "batchCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "fishStockCode", TargetField = "fishStockCode" },
                        new OpeningImportFieldMappingDto { SourceColumn = "fishCount", TargetField = "fishCount" },
                        new OpeningImportFieldMappingDto { SourceColumn = "averageGram", TargetField = "averageGram" },
                    ],
                    Rows =
                    [
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode, ["cageCode"] = $"{projectCode}-C1", ["receiptNo"] = firstReceiptNo,
                            ["receiptDate"] = "2026-04-01", ["batchCode"] = $"{projectCode}-B1", ["fishStockCode"] = "PLAMUT-5G",
                            ["fishCount"] = "500", ["averageGram"] = "5",
                        },
                        new Dictionary<string, string?>
                        {
                            ["projectCode"] = projectCode, ["cageCode"] = $"{projectCode}-C2", ["receiptNo"] = secondReceiptNo,
                            ["receiptDate"] = secondReceiptDate, ["batchCode"] = $"{projectCode}-B1", ["fishStockCode"] = "PLAMUT-5G",
                            ["fishCount"] = "600", ["averageGram"] = "5",
                        }
                    ]
                }
            ]
        };
    }

    private static int InvokeParseInt(string value)
    {
        var method = typeof(OpeningImportService).GetMethod("ParseIntOrDefault", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (int)method!.Invoke(null, [value, 0])!;
    }

    private static decimal InvokeParseDecimal(string value)
    {
        var method = typeof(OpeningImportService).GetMethod("ParseDecimalOrDefault", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (decimal)method!.Invoke(null, [value, 0m])!;
    }

    private static decimal CalculateWeightedFeedCostPerKg(List<GoodsReceiptLine> lines)
    {
        var totals = lines.Aggregate(
            new { Kg = 0m, Amount = 0m },
            (sum, line) => new
            {
                Kg = sum.Kg + ((line.TotalGram ?? 0m) / 1000m),
                Amount = sum.Amount + (line.LocalLineAmount ?? 0m),
            });

        return totals.Kg > 0
            ? Math.Round(totals.Amount / totals.Kg, 3, MidpointRounding.AwayFromZero)
            : 0m;
    }

    private static decimal CalculateWeightedSalePricePerKg(List<ShipmentLine> lines)
    {
        var totals = lines.Aggregate(
            new { Kg = 0m, Amount = 0m },
            (sum, line) => new
            {
                Kg = sum.Kg + (line.BiomassGram / 1000m),
                Amount = sum.Amount + (line.LocalLineAmount ?? 0m),
            });

        return totals.Kg > 0
            ? Math.Round(totals.Amount / totals.Kg, 3, MidpointRounding.AwayFromZero)
            : 0m;
    }
}
