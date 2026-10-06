using System.Net.Http.Json;
using FluentAssertions;
using TeamAvailability.Contracts.OnCall;
using TeamAvailability.Infrastructure.Persistence.Seed;
using TeamAvailability.Tests.Infrastructure;
using Xunit;

namespace TeamAvailability.Tests.OnCall;

[Collection("Integration")]
public sealed class OnCallApiTests(TeamAvailabilityAppFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task CreateOnCallShift_PersistsShift()
    {
        var token = await LoginAsync("lead@teamavailability.local", "Lead123!");
        Authorize(token);

        var start = DateTimeOffset.UtcNow.AddDays(2);
        var end = start.AddHours(8);

        var create = await Client.PostAsJsonAsync(
            $"/api/v1/teams/{SeedIds.DemoTeamId}/on-call",
            new CreateOnCallShiftRequest(SeedIds.AliceMemberId, start, end));

        if (!create.IsSuccessStatusCode)
        {
            var error = await create.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"On-call create failed ({create.StatusCode}): {error}");
        }
        var shift = await create.Content.ReadFromJsonAsync<OnCallShiftResponse>();
        shift!.TeamMemberId.Should().Be(SeedIds.AliceMemberId);
        shift.MemberName.Should().Be("Alice Developer");

        var list = await Client.GetFromJsonAsync<List<OnCallShiftResponse>>(
            $"/api/v1/teams/{SeedIds.DemoTeamId}/on-call");

        list!.Should().Contain(s => s.Id == shift.Id);
    }
}
