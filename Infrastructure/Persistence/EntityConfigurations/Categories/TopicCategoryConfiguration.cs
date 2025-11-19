// Infrastructure/Configurations/TopicCategoryConfiguration.cs
using Domain.Entities.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    /// <summary>
    /// Fluent config matching the SQL: table "TopicCategory" (dbo), NEWSEQUENTIALID, unique Description.
    /// </summary>
    public class TopicCategoryConfiguration : IEntityTypeConfiguration<TopicCategory>
    {
        public void Configure(EntityTypeBuilder<TopicCategory> builder)
        {
            builder.ToTable("TopicCategory");                  // dbo.TopicCategory
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(e => e.Description)
                   .IsRequired()
                   .HasMaxLength(200);

            // Enforce uniqueness at the model level (aligns with DB unique index)
            builder.HasIndex(e => e.Description).IsUnique();
        }
    }
}
