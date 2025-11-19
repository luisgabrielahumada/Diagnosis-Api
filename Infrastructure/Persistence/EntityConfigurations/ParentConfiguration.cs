namespace Infrastructure.Persistence.EntityConfigurations
{
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParentConfiguration : IEntityTypeConfiguration<Parent>
    {
        public void Configure(EntityTypeBuilder<Parent> builder)
        {
            builder.ToTable("Parent");
            builder.HasKey(e => e.Id);

            //builder.HasOne(e => e.User)
            //    .WithOne()
            //    .HasForeignKey<Parent>(e => e.Id)
            //    .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
