using System.Net.Http.Json;
using FluentAssertions;
using TeamAvailability.Contracts.OnCall;
using TeamAvailability.Infrastructure.Persistence.Seed;
using TeamAvailability.Tests.Infrastructure;
using Xunit;

namespace TeamAvailability.Tests.OnCall;

[Collection("JiraIntegration")]
public sealed class OnCallJiraTests(JiraEnabledAppFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task CreateOnCallShift_LinksJiraIssue_WhenJiraEnabled()
    {
        var token = await LoginAsync("lead@teamavailability.local", "Lead123!");
        Authorize(token);

        var start = DateTimeOffset.UtcNow.AddDays(5);
        var end = start.AddHours(8);

        var create = await Client.PostAsJsonAsync(
            $"/api/v1/teams/{SeedIds.DemoTeamId}/on-call",
            new CreateOnCallShiftRequest(SeedIds.AliceMemberId, start, end));

        create.EnsureSuccessStatusCode();
        var shift = await create.Content.ReadFromJsonAsync<OnCallShiftResponse>();

        shift!.JiraIssueKey.Should().Be("TEST-1");
        shift.JiraIssueUrl.Should().Be("https://jira.test/browse/TEST-1");
        factory.FakeJira.CreatedIssues.Should().ContainSingle(i => i.ProjectKey == "KAN");
    }
}
