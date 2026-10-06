using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamAvailability.Domain.Entities;

namespace TeamAvailability.Infrastructure.Persistence.Configurations;

public sealed class AbsenceConfiguration : IEntityTypeConfiguration<Absence>
{
    public void Configure(EntityTypeBuilder<Absence> builder)
    {
        builder.ToTable("absences");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => new { x.TeamMemberId, x.StartDate, x.EndDate })
            .HasFilter("\"IsDeleted\" = false");

        builder.HasOne(x => x.TeamMember).WithMany(m => m.Absences).HasForeignKey(x => x.TeamMemberId);
        builder.HasOne(x => x.ReplacementMember).WithMany().HasForeignKey(x => x.ReplacementMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
