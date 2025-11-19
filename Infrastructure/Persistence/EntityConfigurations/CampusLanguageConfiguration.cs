// CampusLanguageConfiguration.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CampusLanguageConfiguration : IEntityTypeConfiguration<CampusLanguage>
{
    public void Configure(EntityTypeBuilder<CampusLanguage> builder)
    {
        builder.ToTable("CampusLanguage");
        builder.HasKey(cl => new { cl.CampusId, cl.LanguageId });

        builder
            .HasOne(cl => cl.Campus)
            .WithMany(c => c.CampusLanguages)
            .HasForeignKey(cl => cl.CampusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(cl => cl.Language)
            .WithMany(l => l.Campuses)
            .HasForeignKey(cl => cl.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
