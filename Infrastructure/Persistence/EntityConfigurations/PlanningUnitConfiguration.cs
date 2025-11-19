using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Extensions;

namespace Infrastructure.Persistence.EntityConfigurations;

public class PlanningUnitConfiguration : IEntityTypeConfiguration<PlanningUnit>
{
    public void Configure(EntityTypeBuilder<PlanningUnit> builder)
    {
        builder.ToTable("PlanningUnit");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.AcademicUnitId);
        builder.Property(e => e.PlanningId);
        builder.Property(e => e.Name);
        builder.Property(e => e.NameEn);
        builder.Property(e => e.Code)
            .HasMaxLength(100)
            .HasValueGenerator<CodeGeneratorExtensions>();
        builder.Property(e => e.Description);
        builder.Property(e => e.DescriptionEn);
        builder.Property(e => e.UserId);
        builder.Property(e => e.CreatedAt);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.IsDeleted);
        builder.Property(e => e.IsOwner);
    }
}