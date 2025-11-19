using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Extensions;

namespace Infrastructure.Persistence.EntityConfigurations;

public class PlanningMethodologyConfiguration : IEntityTypeConfiguration<PlanningMethodology>
{
    public void Configure(EntityTypeBuilder<PlanningMethodology> builder)
    {
        builder.ToTable("PlanningMethodology");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.AcademicMethodologyId);
        builder.Property(e => e.PlanningId);
        builder.Property(e => e.Name);
        builder.Property(e => e.NameEn);
        builder.Property(e => e.Code)
            .IsRequired()
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