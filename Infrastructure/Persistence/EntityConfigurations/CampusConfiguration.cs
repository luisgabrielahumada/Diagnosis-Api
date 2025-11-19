using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Extensions;

namespace Infrastructure.Persistence.EntityConfigurations;

public class CampusConfiguration : IEntityTypeConfiguration<Campus>
{
    public void Configure(EntityTypeBuilder<Campus> builder)
    {
        builder.ToTable("Campus");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id);
        builder.Property(e => e.Name);
        builder.Property(e => e.Code)
            .HasMaxLength(100)
            .HasValueGenerator<CodeGeneratorExtensions>();
        builder.Property(e => e.Address);
        builder.Property(e => e.Phone);
        builder.Property(e => e.Email);
        builder.Property(e => e.IsActive);
        builder.Property(e => e.CreatedAt);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.IsDeleted);

        builder
          .HasMany(c => c.CampusSubjects)
          .WithOne(cs => cs.Campus)
          .HasForeignKey(cs => cs.CampusId);

        builder
          .HasMany(c => c.CampusSubjects)
          .WithOne(cs => cs.Campus);
    }
}
