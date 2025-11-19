using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Name).HasMaxLength(120);
            builder.Property(u => u.Login).IsRequired().HasMaxLength(120);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(120);
            builder.Property(u => u.Phone).HasMaxLength(50);
            builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
        }
    }
}