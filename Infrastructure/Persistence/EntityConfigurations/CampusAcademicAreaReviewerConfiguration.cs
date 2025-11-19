using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class CampusAcademicAreaReviewerConfiguration
  : IEntityTypeConfiguration<CampusAcademicAreaReviewer>
{
    public void Configure(EntityTypeBuilder<CampusAcademicAreaReviewer> b)
    {
        b.ToTable("CampusAcademicAreaReviewer");
        b.HasKey(x => new { x.CampusId, x.AcademicAreaId, x.ReviewerId });

        b.Property(x => x.CampusId)
         .HasColumnName("Campus_Id");

        b.Property(x => x.AcademicAreaId)
         .HasColumnName("AcademicArea_Id");

        b.Property(x => x.ReviewerId)
         .HasColumnName("Reviewer_Id");

        b.HasOne(x => x.Campus)
         .WithMany(c => c.AreaReviewers)
         .HasForeignKey(x => x.CampusId);

        b.HasOne(x => x.AcademicArea)
         .WithMany(a => a.Reviewers)  // si quieres navegación inversa
         .HasForeignKey(x => x.AcademicAreaId);

        b.HasOne(x => x.Reviewer)
         .WithMany(u => u.ReviewingAssignments)  // o como lo hayas llamado
         .HasForeignKey(x => x.ReviewerId);
    }
}
