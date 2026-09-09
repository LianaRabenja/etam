using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class PrevisionGlobaleConfiguration : IEntityTypeConfiguration<PrevisionGlobale>
{
    public void Configure(EntityTypeBuilder<PrevisionGlobale> b)
    {
        b.ToTable("PrevisionsGlobales");
        b.HasKey(x => x.Id);
        b.Property(x => x.Reference).HasMaxLength(60).IsRequired();
        b.Property(x => x.Observation).HasMaxLength(500);
        b.Property(x => x.MotifRefus).HasMaxLength(500);

        b.HasOne(x => x.Chantier).WithMany()
            .HasForeignKey(x => x.ChantierId).OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Lignes).WithOne(l => l.PrevisionGlobale)
            .HasForeignKey(l => l.PrevisionGlobaleId).OnDelete(DeleteBehavior.Cascade);

        b.Ignore(x => x.Total);
        b.Ignore(x => x.EstModifiable);
        b.Ignore(x => x.EstValidee);
    }
}

public class PrevisionGlobaleLigneConfiguration : IEntityTypeConfiguration<PrevisionGlobaleLigne>
{
    public void Configure(EntityTypeBuilder<PrevisionGlobaleLigne> b)
    {
        b.ToTable("PrevisionsGlobalesLignes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Rubrique).HasMaxLength(100).IsRequired();
        b.Property(x => x.Designation).HasMaxLength(150).IsRequired();
        b.Property(x => x.Unite).HasMaxLength(20);
        b.Property(x => x.Observation).HasMaxLength(300);
        b.Property(x => x.Quantite).HasColumnType("numeric(18,3)");
        b.Property(x => x.PrixUnitaire).HasColumnType("numeric(18,2)");

        b.Ignore(x => x.Total);
    }
}
