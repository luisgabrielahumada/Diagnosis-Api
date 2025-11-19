using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Extensions;

namespace Infrastructure.Persistence.EntityConfigurations;

public class MainCurriculumFrameworkFileConfiguration : IEntityTypeConfiguration<MainCurriculumFrameworkFile>
{
    public void Configure(EntityTypeBuilder<MainCurriculumFrameworkFile> builder)
    {
        builder.ToTable("MainCurriculumFrameworkFile");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.IsActive);
        builder.Property(e => e.CreatedAt);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.IsDeleted);
    }
}