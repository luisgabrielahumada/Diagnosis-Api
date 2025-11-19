using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class ProcessFormReviewConfiguration : IEntityTypeConfiguration<ProcessFormReview>
    {
        public void Configure(EntityTypeBuilder<ProcessFormReview> builder)
        {
            builder.ToTable("ProcessFormReview");
            builder.HasNoKey();
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type)
                   .HasMaxLength(32)
                   .IsRequired();
            builder.Property(x => x.Observations).IsRequired();
            builder.HasIndex(x => x.ProessFormId);
        }
    }
}