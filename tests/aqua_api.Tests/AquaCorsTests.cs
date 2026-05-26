using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace aqua_api.Tests;

public sealed class AquaCorsTests : IClassFixture<AquaHttpTestWebApplicationFactory>
{
    private readonly AquaHttpTestWebApplicationFactory _factory;

    public AquaCorsTests(AquaHttpTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OptionsPreflight_AllowsSameHostDifferentPortCustomerInstall()
    {
        using var response = await SendPreflightAsync("/api/NetsisRead/getBranches", "GET");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("http://10.175.5.61:5173", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.Equal("true", response.Headers.GetValues("Access-Control-Allow-Credentials").Single());
        Assert.Contains("GET", response.Headers.GetValues("Access-Control-Allow-Methods").Single());
        Assert.Contains("Authorization", response.Headers.GetValues("Access-Control-Allow-Headers").Single());
    }

    [Theory]
    [InlineData("/api/aqua/OpeningImport/preview")]
    [InlineData("/api/aqua/OpeningImport/17/cleanup-soft-deleted")]
    [InlineData("/api/aqua/OpeningImport/17/reset-existing-data")]
    [InlineData("/api/aqua/OpeningImport/17/commit")]
    public async Task OptionsPreflight_AllowsOpeningImportCommandsFromCustomerInstall(string path)
    {
        using var response = await SendPreflightAsync(path, "POST");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("http://10.175.5.61:5173", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.Contains("POST", response.Headers.GetValues("Access-Control-Allow-Methods").Single());
        Assert.Contains("Content-Type", response.Headers.GetValues("Access-Control-Allow-Headers").Single());
    }

    [Fact]
    public async Task OpeningImportCommandErrorResponse_PreservesCorsHeaders()
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/aqua/OpeningImport/999999/cleanup-soft-deleted");
        request.Headers.TryAddWithoutValidation("Origin", "http://10.175.5.61:5173");
        request.Headers.TryAddWithoutValidation("Host", "10.175.5.61:5001");
        request.Headers.TryAddWithoutValidation("X-Branch-Code", "1");
        request.Content = JsonContent.Create(new { });

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("http://10.175.5.61:5173", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    private async Task<HttpResponseMessage> SendPreflightAsync(string path, string method)
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, path);
        request.Headers.TryAddWithoutValidation("Origin", "http://10.175.5.61:5173");
        request.Headers.TryAddWithoutValidation("Host", "10.175.5.61:5001");
        request.Headers.TryAddWithoutValidation("Access-Control-Request-Method", method);
        request.Headers.TryAddWithoutValidation("Access-Control-Request-Headers", "content-type,authorization,x-branch-code,x-language");

        var response = await client.SendAsync(request);
        client.Dispose();
        return response;
    }
}
