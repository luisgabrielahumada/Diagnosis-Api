using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class ProcessFormDataConfiguration : IEntityTypeConfiguration<ProcessFormData>
    {
        public void Configure(EntityTypeBuilder<ProcessFormData> builder)
        {
            builder.ToTable("ProcessFormData");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.FamilyInfo);
            builder.Property(u => u.Plan);
            builder.Property(u => u.Priorities);
            builder.Property(u => u.Learning);
            builder.Property(u => u.ActionPlan);
            builder.Property(u => u.ActionPlanHouse);
            builder.Property(u => u.ActionPlanSchool);
            builder.Property(u => u.Observation);
            builder.Property(u => u.Signature);
        }
    }
}