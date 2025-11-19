using Domain.Entities.Virus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DnaMatrixConfiguration : IEntityTypeConfiguration<DnaMatrix>
{
    public void Configure(EntityTypeBuilder<DnaMatrix> builder)
    {
        builder.ToTable("DnaMatrix");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RowIndex)
            .IsRequired();

        builder.Property(x => x.C0).HasColumnType("char(1)").IsRequired();
        builder.Property(x => x.C1).HasColumnType("char(1)").IsRequired();
        builder.Property(x => x.C2).HasColumnType("char(1)").IsRequired();
        builder.Property(x => x.C3).HasColumnType("char(1)").IsRequired();
        builder.Property(x => x.C4).HasColumnType("char(1)").IsRequired();
        builder.Property(x => x.C5).HasColumnType("char(1)").IsRequired();
    }
}
