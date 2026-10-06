using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeamAvailability.Application.Common.Interfaces;
using Testcontainers.PostgreSql;
using Xunit;

namespace TeamAvailability.Tests.Infrastructure;

public class TeamAvailabilityAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _postgres;
    public string ConnectionString { get; private set; } = string.Empty;
    public FakeJiraClient FakeJira { get; } = new();
    public bool EnableJiraIntegration { get; init; }

    public async Task InitializeAsync()
    {
        ConnectionString = await ResolveConnectionStringAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_postgres is not null)
            await _postgres.DisposeAsync();

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["LocalDev:UseMemoryCache"] = "true",
                ["LocalDev:UseInMemoryBus"] = "true",
                ["Jwt:Secret"] = "IntegrationTestsSecretKey_MustBeAtLeast32Chars!",
                ["Jwt:Issuer"] = "TeamAvailability",
                ["Jwt:Audience"] = "TeamAvailabilityClients",
                ["Telegram:BotToken"] = "",
                ["Jira:Enabled"] = EnableJiraIntegration ? "true" : "false"
            };

            if (EnableJiraIntegration)
            {
                settings["Jira:ProjectKey"] = "KAN";
                settings["Jira:IssueTypeId"] = "10003";
                settings["Jira:BaseUrl"] = "https://jira.test";
                settings["Jira:Email"] = "test@test.com";
                settings["Jira:ApiToken"] = "token";
            }

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IJiraClient));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton<IJiraClient>(FakeJira);
        });
    }

    private async Task<string> ResolveConnectionStringAsync()
    {
        var explicitConnection = Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(explicitConnection))
            return explicitConnection;

        if (await TryStartTestcontainerAsync())
            return _postgres!.GetConnectionString();

        var dbName = $"team_availability_test_{Guid.NewGuid():N}";
        return $"Host=localhost;Port=5432;Database={dbName};Username=postgres;Password=light5890apobeg0rl5A";
    }

    private async Task<bool> TryStartTestcontainerAsync()
    {
        try
        {
            _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
            await _postgres.StartAsync();
            return true;
        }
        catch
        {
            _postgres = null;
            return false;
        }
    }
}
