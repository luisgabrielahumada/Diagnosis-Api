using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class ProcessFormConfiguration : IEntityTypeConfiguration<ProcessForm>
    {
        public void Configure(EntityTypeBuilder<ProcessForm> builder)
        {
            builder.ToTable("ProcessForm");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.StudentId);
            builder.Property(u => u.CustomerId);
            builder.Property(u => u.TeacherId);
            builder.Property(u => u.TeachingAssignmentId);
            builder.Property(u => u.Status);
            builder.Property(u => u.CreatedAt);
            builder.Property(u => u.UpdatedAt);
            builder.Property(u => u.Version);
            builder.Property(u => u.InterviewDate);
            builder.Property(u => u.SendMethod);
            builder.Property(u => u.AccessToken);
            builder.Property(u => u.EmailSent);
        }
    }
}