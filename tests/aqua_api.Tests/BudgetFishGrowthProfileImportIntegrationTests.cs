using System.Net;
using System.Net.Http.Json;
using aqua_api.Modules.Budget.Application.Dtos;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace aqua_api.Tests;

public sealed class BudgetFishGrowthProfileImportIntegrationTests : IClassFixture<AquaHttpTestWebApplicationFactory>
{
    private readonly AquaHttpTestWebApplicationFactory _factory;

    public BudgetFishGrowthProfileImportIntegrationTests(AquaHttpTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Import_GroupsRowsByStockAndStartMonth_AndReplacesGrowthCurve()
    {
        var client = _factory.CreateClient();
        var request = new ImportBudgetFishGrowthProfilesDto
        {
            Rows = new List<ImportBudgetFishGrowthProfileRowDto>
            {
                new() { StockCode = "PLAMUT-5G", StartMonth = 1, GrowthMonthNo = 1, MonthlyGrowthGram = 2.00000000m },
                new() { StockCode = "PLAMUT-5G", StartMonth = 1, GrowthMonthNo = 2, MonthlyGrowthGram = 6.33333333m },
                new() { StockCode = "PLAMUT-10G", StartMonth = 2, GrowthMonthNo = 1, MonthlyGrowthGram = 3.25000000m }
            }
        };

        using var response = await client.PostAsJsonAsync("/api/budget/FishGrowthProfile/import", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ImportBudgetFishGrowthProfilesResultDto>>();
        Assert.True(body?.Success, body?.ExceptionMessage);
        Assert.Equal(2, body!.Data!.ProfileCount);
        Assert.Equal(3, body.Data.RowCount);

        long profileId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var profile = await db.BudgetFishGrowthProfiles
                .Include(x => x.Stock)
                .Include(x => x.Lines)
                .SingleAsync(x => x.Stock.ErpStockCode == "PLAMUT-5G" && x.StartMonth == 1);

            profileId = profile.Id;
            Assert.Equal(100, profile.Lines.Count);
            Assert.Equal(2.00000000m, profile.Lines.Single(x => x.GrowthMonthNo == 1).MonthlyGrowthGram);
            Assert.Equal(8.33333333m, profile.Lines.Single(x => x.GrowthMonthNo == 2).TotalGram);
        }

        request.Rows = new List<ImportBudgetFishGrowthProfileRowDto>
        {
            new() { StockCode = "PLAMUT-5G", StartMonth = 1, GrowthMonthNo = 1, MonthlyGrowthGram = 4.00000000m },
            new() { StockCode = "PLAMUT-5G", StartMonth = 1, GrowthMonthNo = 2, MonthlyGrowthGram = 7.50000000m }
        };
        using var updateResponse = await client.PostAsJsonAsync("/api/budget/FishGrowthProfile/import", request);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var profile = await db.BudgetFishGrowthProfiles
                .Include(x => x.Stock)
                .Include(x => x.Lines)
                .SingleAsync(x => x.Stock.ErpStockCode == "PLAMUT-5G" && x.StartMonth == 1);

            Assert.Equal(profileId, profile.Id);
            Assert.Equal(100, profile.Lines.Count);
            Assert.Equal(4.00000000m, profile.Lines.Single(x => x.GrowthMonthNo == 1).MonthlyGrowthGram);
            Assert.Equal(11.50000000m, profile.Lines.Single(x => x.GrowthMonthNo == 2).TotalGram);
            Assert.Equal(0m, profile.Lines.Single(x => x.GrowthMonthNo == 80).MonthlyGrowthGram);
            Assert.Equal(11.50000000m, profile.Lines.Single(x => x.GrowthMonthNo == 100).TotalGram);
        }
    }

    [Fact]
    public async Task Import_RejectsDuplicateStockStartAndElapsedMonth()
    {
        var client = _factory.CreateClient();
        var request = new ImportBudgetFishGrowthProfilesDto
        {
            Rows = new List<ImportBudgetFishGrowthProfileRowDto>
            {
                new() { StockCode = "PLAMUT-5G", StartMonth = 1, GrowthMonthNo = 1, MonthlyGrowthGram = 2m },
                new() { StockCode = "plamut-5g", StartMonth = 1, GrowthMonthNo = 1, MonthlyGrowthGram = 3m }
            }
        };

        using var response = await client.PostAsJsonAsync("/api/budget/FishGrowthProfile/import", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
