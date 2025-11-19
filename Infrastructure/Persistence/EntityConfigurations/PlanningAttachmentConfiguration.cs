using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations;

public class PlanningAttachmentConfiguration : IEntityTypeConfiguration<PlanningAttachment>
{
    public void Configure(EntityTypeBuilder<PlanningAttachment> builder)
    {
        builder.ToTable("PlanningAttachment");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id);
        builder.Property(e => e.FileName);
        builder.Property(e => e.OriginalFileName);
        builder.Property(e => e.FilePath);
        builder.Property(e => e.FileSize);
        builder.Property(e => e.ContentType);
        builder.Property(e => e.PlanningId);
        builder.Property(e => e.UploadedById);
        builder.Property(e => e.CreatedAt);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.IsDeleted);
    }
}