namespace Infrastructure.Persistence.EntityConfigurations
{
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParentStudentConfiguration : IEntityTypeConfiguration<ParentStudent>
    {
        public void Configure(EntityTypeBuilder<ParentStudent> builder)
        {
            builder.ToTable("ParentStudent");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.IsPrimaryContact).HasDefaultValue(false);

            //builder.HasOne(e => e.Parent).WithMany(p => p.ParentStudents)
            //       .HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.Cascade);

            //builder.HasOne(e => e.Student).WithMany(s => s.ParentStudents)
            //       .HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => new { e.ParentId, e.StudentId }).IsUnique();
        }
    }

}
