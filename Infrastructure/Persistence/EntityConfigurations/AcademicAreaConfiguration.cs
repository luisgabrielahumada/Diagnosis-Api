using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Extensions;

namespace Infrastructure.Persistence.EntityConfigurations;

public class AcademicAreaConfiguration : IEntityTypeConfiguration<AcademicArea>
{
    public void Configure(EntityTypeBuilder<AcademicArea> builder)
    {
        builder.ToTable("AcademicArea");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Code)
            .HasMaxLength(100)
            .HasValueGenerator<CodeGeneratorExtensions>();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Color).HasMaxLength(7);
        builder.Property(e => e.DisplayOrder);
        builder.Property(e => e.IsActive);
        builder.Property(e => e.CreatedAt);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.IsDeleted);
    }
}