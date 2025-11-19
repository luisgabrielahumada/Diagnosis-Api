using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations;

public class PlanningConfiguration : IEntityTypeConfiguration<Planning>
{
    public void Configure(EntityTypeBuilder<Planning> builder)
    {
        builder.ToTable("Planning");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id);
        builder.Property(e => e.SubjectId);
        builder.Property(e => e.GradeId);
        builder.Property(e => e.AcademicPeriodId);
        builder.Property(e => e.CampusId);
        builder.Property(e => e.AcademicYear);
        builder.Property(e => e.AcademicAreaId);
        builder.Property(e => e.TeachingTime);
        builder.Property(e => e.TeacherId);
        builder.Property(e => e.Status);
        builder.Property(e => e.CreatedAt);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.IsDeleted);
        builder.Property(e => e.AssessmentTasks);
        builder.Property(e => e.Performance);
        builder.Property(e => e.LinkingQuestions);
        builder.Property(e => e.StartingDate);
        builder.Property(e => e.FinalDate);
        builder.Property(e => e.ScheduleHours);
    }
}