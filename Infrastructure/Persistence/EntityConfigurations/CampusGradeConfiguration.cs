// CampusGradeConfiguration.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CampusGradeConfiguration : IEntityTypeConfiguration<CampusGrade>
{
    public void Configure(EntityTypeBuilder<CampusGrade> builder)
    {
        builder.ToTable("CampusGrade");
        builder.HasKey(cg => new { cg.CampusId, cg.GradeId });

        builder
            .HasOne(cg => cg.Campus)
            .WithMany(c => c.CampusGrades)
            .HasForeignKey(cg => cg.CampusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(cg => cg.Grade)
            .WithMany(g => g.Campuses)
            .HasForeignKey(cg => cg.GradeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
