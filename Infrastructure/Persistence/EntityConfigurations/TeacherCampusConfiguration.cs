namespace Infrastructure.Persistence.EntityConfigurations
{
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class TeacherCampusConfiguration : IEntityTypeConfiguration<TeacherCampus>
    {
        public void Configure(EntityTypeBuilder<TeacherCampus> builder)
        {
            builder.ToTable("TeacherCampus");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.CampusId);
            builder.Property(e => e.TeacherId);
        }
    }

}
