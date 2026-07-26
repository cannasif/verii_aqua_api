using System.Net;
using System.Net.Http.Json;
using aqua_api.Modules.Aqua.Domain.Enums;
using aqua_api.Modules.Projects.Application.Dtos;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace aqua_api.Tests;

public sealed class PagedSearchHttpIntegrationTests : IClassFixture<AquaHttpTestWebApplicationFactory>
{
    private readonly AquaHttpTestWebApplicationFactory _factory;

    public PagedSearchHttpIntegrationTests(AquaHttpTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PagedLists_SearchOwnAndRelatedDisplayFields()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var projectCode = $"SEARCH-{suffix}";
        var projectName = $"Arama Projesi {suffix}";
        var cageCode = $"SC-{suffix}";
        var batchCode = $"SB-{suffix}";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
            var stockId = await db.Stocks.Select(x => x.Id).FirstAsync();
            var project = new Project
            {
                ProjectCode = projectCode,
                ProjectName = projectName,
                StartDate = new DateTime(2026, 1, 1),
                Status = DocumentStatus.Posted,
            };
            var cage = new Cage { CageCode = cageCode, CageName = $"Arama Kafesi {suffix}" };
            db.Projects.Add(project);
            db.Cages.Add(cage);
            await db.SaveChangesAsync();

            db.ProjectCages.Add(new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cage.Id,
                AssignedDate = project.StartDate,
            });
            db.FishBatches.Add(new FishBatch
            {
                ProjectId = project.Id,
                FishStockId = stockId,
                BatchCode = batchCode,
                CurrentAverageGram = 720m,
                StartDate = project.StartDate,
            });
            await db.SaveChangesAsync();
        }

        using var projectResponse = await client.GetAsync(
            $"/api/aqua/Project?pageNumber=1&pageSize=20&search={Uri.EscapeDataString(projectCode)}");
        Assert.Equal(HttpStatusCode.OK, projectResponse.StatusCode);
        var projectBody = await projectResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ProjectDto>>>();
        Assert.Equal(1, projectBody!.Data!.TotalCount);
        Assert.Contains(projectBody!.Data!.Items, item => item.ProjectCode == projectCode);

        using var batchResponse = await client.GetAsync(
            $"/api/aqua/FishBatch?pageNumber=1&pageSize=20&search={Uri.EscapeDataString(projectName)}");
        Assert.Equal(HttpStatusCode.OK, batchResponse.StatusCode);
        var batchBody = await batchResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<FishBatchDto>>>();
        Assert.Equal(1, batchBody!.Data!.TotalCount);
        Assert.Contains(batchBody!.Data!.Items, item => item.BatchCode == batchCode);

        using var cageResponse = await client.GetAsync(
            $"/api/aqua/ProjectCage?pageNumber=1&pageSize=20&search={Uri.EscapeDataString(cageCode)}");
        Assert.Equal(HttpStatusCode.OK, cageResponse.StatusCode);
        var cageBody = await cageResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ProjectCageDto>>>();
        Assert.Equal(1, cageBody!.Data!.TotalCount);
        Assert.Contains(cageBody!.Data!.Items, item => item.CageCode == cageCode);
    }
}
