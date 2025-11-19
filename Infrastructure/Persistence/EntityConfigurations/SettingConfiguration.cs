using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class SettingConfiguration : IEntityTypeConfiguration<Setting>
    {
        public void Configure(EntityTypeBuilder<Setting> builder)
        {
            builder.ToTable("Setting");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Category).IsRequired();
            builder.Property(u => u.Code).HasMaxLength(50);
            builder.Property(u => u.ControlTitle);
            builder.Property(u => u.Description);
            builder.Property(u => u.Value);
            builder.Property(u => u.ControlType);
            builder.Property(u => u.Options);
            builder.Property(u => u.InputType);
            //builder.Property(u => u.CreatedBy);
            //builder.Property(u => u.CreatedAt);
        }
    }
}