using Microsoft.EntityFrameworkCore;
using TeamAvailability.Domain.Entities;

namespace TeamAvailability.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<ApiKey> ApiKeys { get; }
    DbSet<Team> Teams { get; }
    DbSet<TeamMember> TeamMembers { get; }
    DbSet<Absence> Absences { get; }
    DbSet<OnCallShift> OnCallShifts { get; }
    DbSet<MemberPresence> MemberPresences { get; }
    DbSet<AuditEntry> AuditEntries { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<IdempotencyRecord> IdempotencyRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
