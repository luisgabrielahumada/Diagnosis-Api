namespace Infrastructure.Persistence.EntityConfigurations
{
    using System;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.ToTable("Student");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.FirstName).IsRequired().HasMaxLength(200);
            builder.Property(e => e.LastName).IsRequired().HasMaxLength(200);

            //builder.HasOne(e => e.User)
            //    .WithMany()
            //    .HasForeignKey(e => e.UserId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //builder.HasIndex(e => e.UserId)
            //    .IsUnique()
            //    .HasFilter("[UserId] IS NOT NULL");
        }
    }
}
