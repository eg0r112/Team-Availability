using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TeamAvailability.Contracts.Auth;
using TeamAvailability.Tests.Infrastructure;
using Xunit;

namespace TeamAvailability.Tests.Auth;

[Collection("Integration")]
public sealed class AuthApiTests(TeamAvailabilityAppFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Login_ReturnsToken_WhenCredentialsValid()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("lead@teamavailability.local", "Lead123!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Returns400_WhenCredentialsInvalid()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("lead@teamavailability.local", "WrongPassword!"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_CreatesUser_AndCanLogin()
    {
        var email = $"user_{Guid.NewGuid():N}@test.local";

        var register = await Client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest(email, "Test123!@#", "Test User"));

        register.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "Test123!@#"));

        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
