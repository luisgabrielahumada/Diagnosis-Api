// CampusSubjectConfiguration.cs
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CampusSubjectConfiguration : IEntityTypeConfiguration<CampusSubject>
{
    public void Configure(EntityTypeBuilder<CampusSubject> builder)
    {
        builder.ToTable("CampusSubject");
        builder.HasKey(cs => new { cs.CampusId, cs.SubjectId });

        builder
            .HasOne(cs => cs.Campus)
            .WithMany(c => c.CampusSubjects)
            .HasForeignKey(cs => cs.CampusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(cs => cs.Subject)
            .WithMany(s => s.CampusSubjects)
            .HasForeignKey(cs => cs.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
