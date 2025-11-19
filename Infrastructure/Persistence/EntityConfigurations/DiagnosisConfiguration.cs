using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DiagnosisConfiguration : IEntityTypeConfiguration<Diagnosis>
{
    public void Configure(EntityTypeBuilder<Diagnosis> builder)
    {
        builder.ToTable("Diagnosis");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.GeneticCodeHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(x => x.GeneticCodeHash)
            .IsUnique();

        builder.Property(x => x.DiagnosisType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.GeneticCodeJson)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSDATETIME()");

        builder.HasOne(x => x.Patient)
            .WithMany(p => p.Diagnoses)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
