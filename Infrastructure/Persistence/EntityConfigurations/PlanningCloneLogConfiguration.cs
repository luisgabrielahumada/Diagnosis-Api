using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations;

public class PlanningCloneLogConfiguration : IEntityTypeConfiguration<PlanningCloneLog>
{
    public void Configure(EntityTypeBuilder<PlanningCloneLog> builder)
    {
        builder.ToTable("PlanningCloneLog");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.UserEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.HasOne(e => e.SourcePlanning)
                .WithMany(p => p.CloneLogsAsSource)
                .HasForeignKey(e => e.SourcePlanningId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ClonedPlanning)
            .WithMany(p => p.CloneLogsAsTarget)
            .HasForeignKey(e => e.ClonedPlanningId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
