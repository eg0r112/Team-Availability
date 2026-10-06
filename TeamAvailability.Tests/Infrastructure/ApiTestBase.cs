using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TeamAvailability.Contracts.Auth;
using Xunit;

namespace TeamAvailability.Tests.Infrastructure;

public abstract class ApiTestBase : IAsyncLifetime
{
    protected readonly TeamAvailabilityAppFactory Factory;
    protected readonly HttpClient Client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    protected ApiTestBase(TeamAvailabilityAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public virtual Task InitializeAsync() => WaitForSeederAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task WaitForSeederAsync()
    {
        for (var i = 0; i < 30; i++)
        {
            var response = await Client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("lead@teamavailability.local", "Lead123!"));
            if (response.IsSuccessStatusCode)
                return;

            await Task.Delay(500);
        }

        throw new InvalidOperationException("Database seeder did not complete in time.");
    }

    protected async Task<string> LoginAsync(string email, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions);
        return token!.AccessToken;
    }

    protected void Authorize(string accessToken)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}
