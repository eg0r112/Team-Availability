using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamAvailability.Domain.Entities;

namespace TeamAvailability.Infrastructure.Persistence.Configurations;

public sealed class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("team_members");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TeamId, x.UserId }).IsUnique();
        builder.Property(x => x.TimeZoneId).HasMaxLength(64).IsRequired();

        builder.HasOne(x => x.Team).WithMany(t => t.Members).HasForeignKey(x => x.TeamId);
        builder.HasOne(x => x.User).WithMany(u => u.TeamMemberships).HasForeignKey(x => x.UserId);
        builder.HasOne(x => x.Presence).WithOne(p => p.TeamMember).HasForeignKey<MemberPresence>(p => p.TeamMemberId);
    }
}
