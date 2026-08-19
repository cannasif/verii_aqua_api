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
    public async Task PagedPost_InvalidFieldAndDirection_ReturnBadRequest()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Branch-Code", "1");

        using var invalidField = await client.PostAsJsonAsync(
            "/api/aqua/Project/paged",
            new PagedRequest
            {
                Search = "x",
                SearchFields = ["Project.ProjectCode"]
            });
        Assert.Equal(HttpStatusCode.BadRequest, invalidField.StatusCode);

        using var invalidDirection = await client.PostAsJsonAsync(
            "/api/aqua/Project/paged",
            new PagedRequest
            {
                SortBy = "Id",
                SortDirection = "sideways"
            });
        Assert.Equal(HttpStatusCode.BadRequest, invalidDirection.StatusCode);

        var invalidRequests = new object[]
        {
            new { search = "x", searchFields = Array.Empty<string>() },
            new { filters = new[] { new { column = "Missing", @operator = "equals", value = "1" } } },
            new { filters = new[] { new { column = "Id", @operator = "equals", value = "NaN" } } },
            new { filters = new[] { new { column = "StartDate", @operator = "contains", value = "2026" } } },
            new { filterLogic = "xor" },
            new { pageNumber = 1, pageSize = 501 },
        };

        foreach (var invalidRequest in invalidRequests)
        {
            using var response = await client.PostAsJsonAsync("/api/aqua/Project/paged", invalidRequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
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
        long projectId;
        long projectCageId;
        long batchId;

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

            var projectCage = new ProjectCage
            {
                ProjectId = project.Id,
                CageId = cage.Id,
                AssignedDate = project.StartDate,
            };
            var batch = new FishBatch
            {
                ProjectId = project.Id,
                FishStockId = stockId,
                BatchCode = batchCode,
                CurrentAverageGram = 720m,
                StartDate = project.StartDate,
            };
            db.ProjectCages.Add(projectCage);
            db.FishBatches.Add(batch);
            await db.SaveChangesAsync();
            projectId = project.Id;
            projectCageId = projectCage.Id;
            batchId = batch.Id;
        }

        using var projectResponse = await client.PostAsJsonAsync(
            "/api/aqua/Project/paged",
            new PagedRequest { PageNumber = 1, PageSize = 500 });
        Assert.Equal(HttpStatusCode.OK, projectResponse.StatusCode);
        var projectBody = await projectResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ProjectDto>>>();
        Assert.Contains(projectBody!.Data!.Items, item => item.ProjectCode == projectCode);

        using var projectPostResponse = await client.PostAsJsonAsync(
            "/api/aqua/Project/paged",
            new PagedRequest
            {
                PageNumber = 1,
                PageSize = 20,
                Search = projectId.ToString(),
                SearchFields = ["Id"],
                SortBy = "ProjectCode",
                SortDirection = "asc"
            });
        Assert.Equal(HttpStatusCode.OK, projectPostResponse.StatusCode);
        var projectPostBody = await projectPostResponse.Content
            .ReadFromJsonAsync<ApiResponse<PagedResponse<ProjectDto>>>();
        Assert.Equal(1, projectPostBody!.Data!.TotalCount);
        Assert.Contains(projectPostBody.Data.Items, item => item.ProjectCode == projectCode);

        using var batchResponse = await client.PostAsJsonAsync(
            "/api/aqua/FishBatch/paged",
            new PagedRequest
            {
                PageNumber = 1,
                PageSize = 20,
                Search = batchId.ToString(),
                SearchFields = ["Id"]
            });
        Assert.Equal(HttpStatusCode.OK, batchResponse.StatusCode);
        var batchBody = await batchResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<FishBatchDto>>>();
        Assert.Equal(1, batchBody!.Data!.TotalCount);
        Assert.Contains(batchBody!.Data!.Items, item => item.BatchCode == batchCode);

        using var cageResponse = await client.PostAsJsonAsync(
            "/api/aqua/ProjectCage/paged",
            new PagedRequest
            {
                PageNumber = 1,
                PageSize = 20,
                Search = projectCageId.ToString(),
                SearchFields = ["Id"]
            });
        Assert.Equal(HttpStatusCode.OK, cageResponse.StatusCode);
        var cageBody = await cageResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ProjectCageDto>>>();
        Assert.Equal(1, cageBody!.Data!.TotalCount);
        Assert.Contains(cageBody!.Data!.Items, item => item.CageCode == cageCode);
    }
}
