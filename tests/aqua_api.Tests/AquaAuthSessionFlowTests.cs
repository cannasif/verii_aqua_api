using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using aqua_api.Modules.Identity.Application.Dtos;
using aqua_api.Modules.Identity.Application.Services;
using aqua_api.Shared.Common.Dtos;
using aqua_api.Shared.Infrastructure.Persistence.Data;
using Xunit;

namespace aqua_api.Tests;

public sealed class AquaAuthSessionFlowTests : IClassFixture<AquaJwtHttpTestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AquaJwtHttpTestWebApplicationFactory _factory;

    public AquaAuthSessionFlowTests(AquaJwtHttpTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LoginAndSessionFlow_SecondLoginRevokesFirstToken()
    {
        var client = _factory.CreateClient();

        var firstLogin = await PostAsync<LoginWithSessionResponseDto>(client, "/api/auth/login", new LoginRequest
        {
            Email = "integration-user@example.com",
            Password = "P@ssw0rd!",
            RememberMe = true,
        });

        Assert.True(firstLogin.Success, $"{firstLogin.Message} | {firstLogin.ExceptionMessage}");
        Assert.NotNull(firstLogin.Data);
        Assert.False(string.IsNullOrWhiteSpace(firstLogin.Data!.Token));

        using (var authClient = _factory.CreateClient())
        {
            authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", firstLogin.Data.Token);
            var profile = await GetAsync<UserDto>(authClient, "/api/auth/user");
            Assert.True(profile.Success, $"{profile.Message} | {profile.ExceptionMessage}");
            Assert.Equal("integration-user@example.com", profile.Data!.Email);
        }

        var secondLogin = await PostAsync<LoginWithSessionResponseDto>(client, "/api/auth/login", new LoginRequest
        {
            Email = "integration-user@example.com",
            Password = "P@ssw0rd!",
            RememberMe = false,
        });

        Assert.True(secondLogin.Success, $"{secondLogin.Message} | {secondLogin.ExceptionMessage}");
        Assert.NotEqual(firstLogin.Data.Token, secondLogin.Data!.Token);

        using (var staleClient = _factory.CreateClient())
        {
            staleClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", firstLogin.Data.Token);
            var staleResponse = await staleClient.GetAsync("/api/auth/user");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, staleResponse.StatusCode);
        }

        using (var validClient = _factory.CreateClient())
        {
            validClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secondLogin.Data.Token);
            var validProfile = await GetAsync<UserDto>(validClient, "/api/auth/user");
            Assert.True(validProfile.Success, $"{validProfile.Message} | {validProfile.ExceptionMessage}");
        }

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AquaDbContext>();
        var sessions = await db.UserSessions
            .Where(x => !x.IsDeleted && x.UserId == 1)
            .OrderBy(x => x.Id)
            .ToListAsync();

        Assert.Equal(2, sessions.Count);
        Assert.NotNull(sessions[0].RevokedAt);
        Assert.Null(sessions[1].RevokedAt);
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
}
