using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class CampusSubjectTeacherConfiguration : IEntityTypeConfiguration<CampusSubjectTeacher>
{
    public void Configure(EntityTypeBuilder<CampusSubjectTeacher> builder)
    {
        builder.ToTable("CampusSubjectTeacher");
        builder.HasKey(x => new { x.CampusId, x.SubjectId, x.TeacherId });

        builder.HasOne(x => x.Campus)
               .WithMany(c => c.SubjectTeachers)
               .HasForeignKey(x => x.CampusId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Subject)
               .WithMany(s => s.SubjectTeachers)
               .HasForeignKey(x => x.SubjectId)
               .OnDelete(DeleteBehavior.Cascade);

        //builder.HasOne(x => x.Teacher)
        //       .WithMany(u => u.TeachingAssignments)
        //       .HasForeignKey(x => x.TeacherId)
        //       .OnDelete(DeleteBehavior.Restrict);
    }
}
