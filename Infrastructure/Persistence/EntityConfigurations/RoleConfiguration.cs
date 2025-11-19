using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Role");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Name).IsRequired().HasMaxLength(120);
            builder.Property(u => u.IsDeleted);
            builder.Property(u => u.Code ).IsRequired();
            builder.Property(u => u.IsDeleted);
            builder.Property(u => u.IsEnabledSetting);
        }
    }
}