using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations;

public class MainCurriculumFrameworkConfiguration : IEntityTypeConfiguration<MainCurriculumFramework>
{
    public void Configure(EntityTypeBuilder<MainCurriculumFramework> builder)
    {
        builder.ToTable("MainCurriculumFramework");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.IsActive);
        builder.Property(e => e.CreatedAt);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.IsDeleted);
    }
}