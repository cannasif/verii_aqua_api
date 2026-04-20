using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using aqua_api.Modules.Aqua.Application.Dtos;
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
    public async Task ConcurrentFeedingLineAutoHeader_CreatesSingleHeaderAndKeepsBothLines()
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
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            projectId = await db.Projects.Where(x => !x.IsDeleted && x.ProjectCode == projectCode).Select(x => x.Id).SingleAsync();
            feedStockId = await db.Stocks.Where(x => !x.IsDeleted && x.ErpStockCode == "YEM-STD").Select(x => x.Id).SingleAsync();
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
        Assert.All(results, result => Assert.True(result.Success, $"{result.Message} | {result.ExceptionMessage}"));

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var headers = await verifyDb.Feedings
            .Where(x => !x.IsDeleted && x.ProjectId == projectId && x.FeedingDate.Date == date.Date && x.FeedingSlot == FeedingSlot.Morning)
            .ToListAsync();
        var lines = await verifyDb.FeedingLines
            .Where(x => !x.IsDeleted && headers.Select(h => h.Id).Contains(x.FeedingId))
            .ToListAsync();

        Assert.Single(headers);
        Assert.Equal(2, lines.Count);
        Assert.Equal(22_000m, lines.Sum(x => x.TotalGram));
    }

    private static async Task<ApiResponse<T>> PostAsync<T>(HttpClient client, string url, object payload)
    {
        using var response = await client.PostAsJsonAsync(url, payload);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        Assert.NotNull(body);
        return body!;
    }
}
