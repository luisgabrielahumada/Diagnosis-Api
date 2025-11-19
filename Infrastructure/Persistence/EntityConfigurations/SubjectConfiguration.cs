using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Extensions;

namespace Infrastructure.Persistence.EntityConfigurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subject");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id);
        builder.Property(e => e.AcademicAreaId);
        builder.Property(e => e.Name);
        builder.Property(e => e.Code)
           .IsRequired()
           .HasMaxLength(100)
           .HasValueGenerator<CodeGeneratorExtensions>();
        builder.Property(e => e.WeeklyHours);
        builder.Property(e => e.IsBilingual);
        builder.Property(e => e.Description);
        builder.Property(e => e.IsActive);
        builder.Property(e => e.CreatedAt);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.IsDeleted);
    }
}