using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Persistence.Configuration
{
    public class ScheduledTaskConfiguration : IEntityTypeConfiguration<ScheduledTask>
    {
        public void Configure(EntityTypeBuilder<ScheduledTask> builder)
        {
            builder.ToTable("ScheduledTask");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(x => x.CronExpression)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.MethodName)
                .IsRequired();

            builder.Property(x => x.Module)
                .IsRequired();

            builder.Property(x => x.QueueName);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("SYSDATETIME()");
        }
    }
}