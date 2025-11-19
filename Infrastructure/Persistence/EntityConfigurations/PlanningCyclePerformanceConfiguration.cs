using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Extensions;
using System.Reflection.Emit;

namespace Infrastructure.Persistence.EntityConfigurations;

public class PlanningCyclePerformanceConfiguration : IEntityTypeConfiguration<PlanningCyclePerformance>
{
    public void Configure(EntityTypeBuilder<PlanningCyclePerformance> builder)
    {
        builder.ToTable("PlanningCyclePerformance");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id);
        builder.Property(e => e.PlanningCycleId).IsRequired();
        builder.Property(e => e.PlanningPerformanceId);
        builder.Property(e => e.CreatedAt);
        builder.Property(e => e.CreatedBy);
        builder.Property(e => e.Sequence);
        
    }
}