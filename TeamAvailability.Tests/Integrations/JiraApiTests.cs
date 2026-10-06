using System.Net.Http.Json;
using FluentAssertions;
using TeamAvailability.Contracts.Integrations;
using TeamAvailability.Infrastructure.Persistence.Seed;
using TeamAvailability.Tests.Infrastructure;
using Xunit;

namespace TeamAvailability.Tests.Integrations;

[Collection("Integration")]
public sealed class JiraApiTests(TeamAvailabilityAppFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task GetJiraAssignable_ReturnsAvailableAndUnavailableMembers()
    {
        var token = await LoginAsync("lead@teamavailability.local", "Lead123!");
        Authorize(token);

        var response = await Client.GetAsync($"/api/v1/integrations/jira/teams/{SeedIds.DemoTeamId}/assignable");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JiraAssignableResponse>();
        result!.TeamId.Should().Be(SeedIds.DemoTeamId);
        result.Available.Should().NotBeEmpty();
        result.Available.Select(m => m.DisplayName).Should().Contain("Alice Developer");
    }
}
