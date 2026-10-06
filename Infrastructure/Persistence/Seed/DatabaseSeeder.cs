using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Domain.Entities;
using TeamAvailability.Domain.Enums;

namespace TeamAvailability.Infrastructure.Persistence.Seed;

public static class SeedIds
{
    public static readonly Guid DemoTeamId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AliceMemberId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid BobMemberId = Guid.Parse("33333333-3333-3333-3333-333333333333");
}

public sealed class DatabaseSeeder(
    IServiceScopeFactory scopeFactory,
    ILogger<DatabaseSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await DatabaseBootstrap.EnsureDatabaseExistsAsync(configuration, cancellationToken);
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Users.AnyAsync(cancellationToken))
        {
            await LinkDemoTelegramAsync(db, cancellationToken);
            return;
        }

        var admin = User.Create("admin@teamavailability.local", "Admin User", passwordHasher.Hash("Admin123!"), SystemRole.Admin);
        var lead = User.Create("lead@teamavailability.local", "Team Lead", passwordHasher.Hash("Lead123!"), SystemRole.TeamLead);
        lead.LinkTelegram("5143241640");
        var dev1 = User.Create("dev1@teamavailability.local", "Alice Developer", passwordHasher.Hash("Dev123!"));
        var dev2 = User.Create("dev2@teamavailability.local", "Bob Developer", passwordHasher.Hash("Dev123!"));

        db.Users.AddRange(admin, lead, dev1, dev2);

        var team = Team.Create("Platform Team", "Core backend platform squad");
        SetEntityId(team, SeedIds.DemoTeamId);
        db.Teams.Add(team);

        var leadMember = TeamMember.Create(team.Id, lead.Id, "Europe/Moscow", TeamRole.TeamLead);
        var member1 = TeamMember.Create(team.Id, dev1.Id, "Europe/Moscow");
        SetEntityId(member1, SeedIds.AliceMemberId);
        var member2 = TeamMember.Create(team.Id, dev2.Id, "America/New_York");
        SetEntityId(member2, SeedIds.BobMemberId);
        db.TeamMembers.AddRange(leadMember, member1, member2);

        db.MemberPresences.Add(MemberPresence.Create(member1.Id, PresenceStatus.Working));

        var apiKeyPlain = $"tav_{Guid.NewGuid():N}";
        var apiKey = ApiKey.Create("Jira Integration", passwordHasher.Hash(apiKeyPlain), apiKeyPlain[..8], team.Id, null);
        db.ApiKeys.Add(apiKey);

        await db.SaveChangesAsync(cancellationToken);

        logger.LogWarning("Seeded demo data. Demo API key (save it now): {ApiKey}", apiKeyPlain);
        logger.LogWarning("Demo users: admin@teamavailability.local / Admin123!");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task LinkDemoTelegramAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        var lead = await db.Users.FirstOrDefaultAsync(
            u => u.Email == "lead@teamavailability.local" && u.TelegramChatId == null,
            cancellationToken);

        if (lead is not null)
        {
            lead.LinkTelegram("5143241640");
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static void SetEntityId<T>(T entity, Guid id) where T : Domain.Common.BaseEntity
    {
        typeof(T).GetProperty(nameof(Domain.Common.BaseEntity.Id))!.SetValue(entity, id);
    }
}
