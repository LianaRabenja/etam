using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class ApprovisionnementConfiguration : IEntityTypeConfiguration<Approvisionnement>
{
    public void Configure(EntityTypeBuilder<Approvisionnement> b)
    {
        b.ToTable("Approvisionnements");
        b.HasKey(x => x.Id);
        b.Property(x => x.Reference).HasMaxLength(60).IsRequired();
        b.HasIndex(x => x.Reference).IsUnique();
        b.Property(x => x.Statut).HasConversion<int>();
        b.Property(x => x.Observation).HasMaxLength(1000);

        b.HasOne(x => x.Chantier).WithMany()
            .HasForeignKey(x => x.ChantierId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PrevisionJournaliere).WithMany()
            .HasForeignKey(x => x.PrevisionJournaliereId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => new { x.ChantierId, x.DateAppro });

        b.Ignore(x => x.Total);
        b.Ignore(x => x.EstModifiable);
    }
}

public class ApprovisionnementLigneConfiguration : IEntityTypeConfiguration<ApprovisionnementLigne>
{
    public void Configure(EntityTypeBuilder<ApprovisionnementLigne> b)
    {
        b.ToTable("ApprovisionnementLignes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Designation).HasMaxLength(150).IsRequired();
        b.Property(x => x.Categorie).HasMaxLength(80).IsRequired();
        b.Property(x => x.TypeBudget).HasConversion<int>();
        b.Property(x => x.Quantite).HasColumnType("numeric(18,3)");
        b.Property(x => x.PrixUnitaireEstime).HasColumnType("numeric(18,2)");
        b.Property(x => x.Observation).HasMaxLength(500);

        b.HasOne(x => x.Approvisionnement).WithMany(a => a.Lignes)
            .HasForeignKey(x => x.ApprovisionnementId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Materiau).WithMany()
            .HasForeignKey(x => x.MateriauId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.DetteFournisseur).WithMany()
            .HasForeignKey(x => x.DetteFournisseurId).OnDelete(DeleteBehavior.SetNull);

        b.Ignore(x => x.Total);
    }
}
