using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Extensions;

namespace Infrastructure.Persistence.EntityConfigurations;

public class CycleKnowledgeConfiguration : IEntityTypeConfiguration<CycleKnowledge>
{
    public void Configure(EntityTypeBuilder<CycleKnowledge> builder)
    {
        builder.ToTable("CycleKnowledge");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id);
        builder.Property(e => e.AcademicEssentialKnowledgeId);
       // builder.Property(e => e.PlanningCycleId);
        builder.Property(e => e.CycleObjectiveId);
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
        //builder.Ignore(c => c.PlanningCycleId);
    }
}