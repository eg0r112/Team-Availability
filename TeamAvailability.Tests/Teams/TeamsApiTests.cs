using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TeamAvailability.Contracts.Teams;
using TeamAvailability.Infrastructure.Persistence.Seed;
using TeamAvailability.Tests.Infrastructure;
using Xunit;

namespace TeamAvailability.Tests.Teams;

[Collection("Integration")]
public sealed class TeamsApiTests(TeamAvailabilityAppFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task GetTeams_RequiresAuthentication()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.GetAsync("/api/v1/teams");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTeams_ReturnsSeededTeam_WhenAuthenticated()
    {
        var token = await LoginAsync("lead@teamavailability.local", "Lead123!");
        Authorize(token);

        var response = await Client.GetAsync("/api/v1/teams");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var teams = await response.Content.ReadFromJsonAsync<List<TeamSummaryResponse>>();
        teams.Should().ContainSingle(t => t.Id == SeedIds.DemoTeamId && t.Name == "Platform Team");
    }

    [Fact]
    public async Task GetAvailability_ReturnsMembers_ForSeededTeam()
    {
        var token = await LoginAsync("lead@teamavailability.local", "Lead123!");
        Authorize(token);

        var response = await Client.GetAsync($"/api/v1/teams/{SeedIds.DemoTeamId}/availability");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Alice Developer");
        body.Should().Contain("Bob Developer");
        body.Should().Contain("Team Lead");
    }
}
