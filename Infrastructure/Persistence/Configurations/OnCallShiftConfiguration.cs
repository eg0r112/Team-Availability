using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamAvailability.Domain.Entities;

namespace TeamAvailability.Infrastructure.Persistence.Configurations;

public sealed class OnCallShiftConfiguration : IEntityTypeConfiguration<OnCallShift>
{
    public void Configure(EntityTypeBuilder<OnCallShift> builder)
    {
        builder.ToTable("on_call_shifts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.JiraIssueKey).HasMaxLength(32);
        builder.HasIndex(x => new { x.TeamId, x.StartUtc, x.EndUtc });

        builder.HasOne(x => x.Team).WithMany(t => t.OnCallShifts).HasForeignKey(x => x.TeamId);
        builder.HasOne(x => x.TeamMember).WithMany().HasForeignKey(x => x.TeamMemberId);
    }
}
