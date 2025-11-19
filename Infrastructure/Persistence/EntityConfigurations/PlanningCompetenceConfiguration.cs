using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Extensions;

namespace Infrastructure.Persistence.EntityConfigurations;

public class PlanningCompetenceConfiguration : IEntityTypeConfiguration<PlanningCompetence>
{
    public void Configure(EntityTypeBuilder<PlanningCompetence> builder)
    {
        builder.ToTable("PlanningCompetence");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.CompetenceId);
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