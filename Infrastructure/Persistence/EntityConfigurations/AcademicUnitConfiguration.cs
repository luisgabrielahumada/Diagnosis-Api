using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Extensions;

namespace Infrastructure.Persistence.EntityConfigurations;

public class AcademicUnitConfiguration : IEntityTypeConfiguration<AcademicUnit>
{
    public void Configure(EntityTypeBuilder<AcademicUnit> builder)
    {
        builder.ToTable("AcademicUnit");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id);
        //builder.Property(e => e.CompetencyId);
        builder.Property(e => e.AcademicAreaId);
        builder.Property(e => e.GradeId);
        builder.Property(e => e.AcademicPeriodId);
        builder.Property(e => e.LanguageId);
        builder.Property(e => e.SubjectId);
        builder.Property(e => e.Priority);
        builder.Property(e => e.Name);
        builder.Property(e => e.NameEn);
        builder.Property(e => e.Code)
            .HasMaxLength(100)
            .HasValueGenerator<CodeGeneratorExtensions>();
        builder.Property(e => e.Description);
        builder.Property(e => e.DescriptionEn);
        builder.Property(e => e.EstimatedHours);
        builder.Property(e => e.DisplayOrder);
        builder.Property(e => e.CreatedAt);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.IsDeleted);
    }
}