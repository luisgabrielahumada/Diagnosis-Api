using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Extensions;
using System.Reflection.Emit;

namespace Infrastructure.Persistence.EntityConfigurations;

public class PlanningCycleConfiguration : IEntityTypeConfiguration<PlanningCycle>
{
    public void Configure(EntityTypeBuilder<PlanningCycle> builder)
    {
        builder.ToTable("PlanningCycle");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id);
        builder.Property(e => e.PlanningId).IsRequired();
        builder.Property(e => e.StartingDate);
        builder.Property(e => e.FinalDate);
        builder.Property(e => e.Session);
        builder.Property(e => e.Activity);
        builder.Property(e => e.ActivityEn);
        builder.Property(e => e.Code)
            .HasMaxLength(100)
            .HasValueGenerator<CodeGeneratorExtensions>();
        builder.Property(e => e.Resources);
        builder.Property(e => e.ResourcesEn);
        builder.Property(e => e.Observations);
        builder.Property(e => e.ObservationsEn);
        builder.Property(e => e.UserId);
        builder.Property(e => e.CreatedAt);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.IsDeleted);
        builder.Property(e => e.IsOwner);
        builder.Property(e => e.IsApproved);
        builder.Property(e => e.Name);
        builder.Property(e => e.Order);
        builder.Property(e => e.LinkingQuestions);
        //builder.Property(e => e.PlanningPerformanceId);

        builder
                .HasMany(c => c.CyclePerformances)
                .WithOne(cp => cp.PlanningCycle)
                .HasForeignKey(cp => cp.PlanningCycleId)
                .OnDelete(DeleteBehavior.Cascade);

    }
}