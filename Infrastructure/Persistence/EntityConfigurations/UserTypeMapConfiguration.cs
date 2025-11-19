using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class UserTypeMapConfiguration : IEntityTypeConfiguration<UserTypeMap>
    {
        public void Configure(EntityTypeBuilder<UserTypeMap> builder)
        {
            builder.ToTable("UserTypeMap");

            builder.HasNoKey();
            builder.HasKey(x => new { x.UserId, x.Kind });

            builder.Property(x => x.Kind)
                   .HasMaxLength(32)
                   .IsRequired();

            builder.Property(x => x.EntityId).IsRequired();

            // Índice de consulta rápida por UserId
            builder.HasIndex(x => x.UserId);

            // FK a User (ajusta DeleteBehavior según tu diseño)
            builder.HasOne(x => x.User)
                   .WithMany(u => u.TypeMaps)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}