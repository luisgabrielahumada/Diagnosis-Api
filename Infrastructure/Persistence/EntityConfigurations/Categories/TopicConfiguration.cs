// Infrastructure/Configurations/TopicConfiguration.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    /// <summary>
    /// Fluent config matching the SQL: table "Topic" (dbo), FK to TopicCategory, unique (CategoryId, Description).
    /// </summary>
    public class TopicConfiguration : IEntityTypeConfiguration<Topic>
    {
        public void Configure(EntityTypeBuilder<Topic> builder)
        {
            builder.ToTable("Topic");                           // dbo.Topic
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(e => e.CategoryId);               // FK column

            builder.Property(e => e.Description)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasOne(e => e.Category)
                   .WithMany(c => c.Topics)
                   .HasForeignKey(e => e.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);        // Conservative: no cascade deletes

            // Useful index for queries by category
            builder.HasIndex(e => e.CategoryId);

            // Unique per category (matches DB UQ constraint)
            builder.HasIndex(e => new { e.CategoryId, e.Description })
                   .IsUnique();
        }
    }
}
