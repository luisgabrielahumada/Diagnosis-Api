// CampusAcademicAreaConfiguration.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CampusAcademicAreaConfiguration : IEntityTypeConfiguration<CampusAcademicArea>
{
    public void Configure(EntityTypeBuilder<CampusAcademicArea> builder)
    {
        builder.ToTable("CampusAcademicArea");
        builder.HasKey(ca => new { ca.CampusId, ca.AcademicAreaId });

        builder
            .HasOne(ca => ca.Campus)
            .WithMany(c => c.CampusAcademicAreas)
            .HasForeignKey(ca => ca.CampusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(ca => ca.AcademicArea)
            .WithMany(a => a.Campuses)
            .HasForeignKey(ca => ca.AcademicAreaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
