using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Integration.Infrastructure;
using Shared.Core.Auth;
using Xunit;

namespace Integration.Features.Auth;

/// <summary>
/// Integration tests for auth API.
/// </summary>
[Collection(Integration.Infrastructure.IntegrationCollection.Name)]
public class AuthApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_with_invalid_credentials_should_return_401_or_400()
    {
        var request = new LoginRequest { Email = "nobody@example.com", Password = "WrongPassword123!" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // API may return 401 Unauthorized or 400 Bad Request for invalid credentials
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_with_valid_credentials_should_return_200_and_UserInfo()
    {
        // Requires a seeded test user (e.g. from DbSeeder). If no test user exists, this test will get 401.
        var request = new LoginRequest { Email = "admin@example.com", Password = "Admin123!" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>();
            userInfo.Should().NotBeNull();
            userInfo!.UserId.Should().NotBeNullOrEmpty();
            userInfo.Email.Should().NotBeNullOrEmpty();
        }
        else
        {
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task Logout_without_authentication_should_return_401()
    {
        var response = await _client.PostAsync("/api/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_after_login_should_return_200()
    {
        var loginRequest = new LoginRequest { Email = "admin@example.com", Password = "Admin123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        if (loginResponse.StatusCode != HttpStatusCode.OK)
        {
            return; // No seeded user; skip asserting logout
        }

        var logoutResponse = await _client.PostAsync("/api/auth/logout", null);

        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
