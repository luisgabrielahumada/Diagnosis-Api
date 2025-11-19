namespace Infrastructure.Persistence.EntityConfigurations
{
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentEnrollmentConfiguration : IEntityTypeConfiguration<StudentEnrollment>
    {
        public void Configure(EntityTypeBuilder<StudentEnrollment> builder)
        {
            builder.ToTable("StudentEnrollment");
            builder.HasKey(e => e.Id);

            //builder.Property(e => e.EnrolledAt).HasDefaultValueSql("SYSUTCDATETIME()");

            //builder.HasOne(e => e.Student).WithMany(s => s.Enrollments)
            //       .HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Cascade);

            //builder.HasOne(e => e.Campus).WithMany()
            //       .HasForeignKey(e => e.CampusId).OnDelete(DeleteBehavior.Restrict);

            //builder.HasOne(e => e.CampusGrade).WithMany()
            //       .HasForeignKey(e => e.CampusGradeId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => new { e.StudentId, e.CampusId }).IsUnique();
            builder.HasIndex(e => new { e.StudentId, e.CampusGradeId }).IsUnique();
        }
    }

}
