namespace Infrastructure.Persistence.EntityConfigurations
{
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TeachingAssignmentConfiguration : IEntityTypeConfiguration<TeachingAssignment>
    {
        public void Configure(EntityTypeBuilder<TeachingAssignment> builder)
        {
            builder.ToTable("TeachingAssignment");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.AcademicPeriodId);
            builder.Property(e => e.CampusGradeId);
            builder.Property(e => e.TeacherId);
            //builder.HasOne(e => e.Teacher).WithMany(t => t.TeachingAssignments)
            //       .HasForeignKey(e => e.TeacherId).OnDelete(DeleteBehavior.Cascade);

            //builder.HasOne(e => e.CampusGrade).WithMany()
            //       .HasForeignKey(e => e.CampusGradeId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => new { e.TeacherId, e.CampusGradeId, e.TeacherType }).IsUnique();
        }
    }

}
