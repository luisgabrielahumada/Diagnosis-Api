// CampusCourseConfiguration.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CampusCourseConfiguration : IEntityTypeConfiguration<CampusCourse>
{
    public void Configure(EntityTypeBuilder<CampusCourse> builder)
    {
        builder.ToTable("CampusCourse");
        builder.HasKey(cc => new { cc.CampusId, cc.CourseId });

        builder
            .HasOne(cc => cc.Campus)
            .WithMany(c => c.CampusCourses)
            .HasForeignKey(cc => cc.CampusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(cc => cc.Course)
            .WithMany(cou => cou.Campuses)
            .HasForeignKey(cc => cc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
