using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations;

public class CycleReviewConfiguration : IEntityTypeConfiguration<CycleReview>
{
    public void Configure(EntityTypeBuilder<CycleReview> builder)
    {
        builder.ToTable("CycleReview");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id);
        builder.Property(e => e.Status);
        builder.Property(e => e.Comments);
        builder.Property(e => e.InternalNotes);
        builder.Property(e => e.ReviewedAt);
        builder.Property(e => e.PlanningCycleId);
        builder.Property(e => e.CreatedAt);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.IsDeleted);
    }
}