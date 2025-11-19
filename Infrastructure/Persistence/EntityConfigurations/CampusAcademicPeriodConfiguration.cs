// CampusAcademicPeriodConfiguration.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CampusAcademicPeriodConfiguration : IEntityTypeConfiguration<CampusAcademicPeriod>
{
    public void Configure(EntityTypeBuilder<CampusAcademicPeriod> builder)
    {
        builder.ToTable("CampusAcademicPeriod");
        builder.HasKey(cp => new { cp.CampusId, cp.AcademicPeriodId });

        builder
            .HasOne(cp => cp.Campus)
            .WithMany(c => c.CampusAcademicPeriods)
            .HasForeignKey(cp => cp.CampusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(cp => cp.AcademicPeriod)
            .WithMany(ap => ap.Campuses)
            .HasForeignKey(cp => cp.AcademicPeriodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
