using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class PrevisionJournaliereConfiguration : IEntityTypeConfiguration<PrevisionJournaliere>
{
    public void Configure(EntityTypeBuilder<PrevisionJournaliere> b)
    {
        b.ToTable("Previsions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Reference).HasMaxLength(60).IsRequired();
        b.HasIndex(x => x.Reference).IsUnique();
        b.Property(x => x.Observation).HasMaxLength(1000);
        b.Property(x => x.MotifRefus).HasMaxLength(500);
        b.Property(x => x.Statut).HasConversion<int>();
        b.Property(x => x.RapportRealisation).HasMaxLength(2000);
        b.Property(x => x.MotifRefusRapport).HasMaxLength(500);
        b.Property(x => x.AccuseNomSignataire).HasMaxLength(150);
        b.Property(x => x.MontantAccuse).HasColumnType("numeric(18,2)");
        b.Property(x => x.ReportVeille).HasColumnType("numeric(18,2)");
        b.Property(x => x.MontantDecaisse).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Chantier).WithMany(c => c.Previsions)
            .HasForeignKey(x => x.ChantierId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.ChantierId, x.DatePrevision });

        // Rattachement à l'enveloppe du mois.
        b.HasOne(x => x.PrevisionMensuelle).WithMany(m => m.PrevisionsJournalieres)
            .HasForeignKey(x => x.PrevisionMensuelleId).OnDelete(DeleteBehavior.Restrict);

        // Chaînage des journées : le report de mardi pointe vers lundi.
        b.HasOne(x => x.PrevisionPrecedente).WithMany()
            .HasForeignKey(x => x.PrevisionPrecedenteId).OnDelete(DeleteBehavior.Restrict);

        b.Ignore(x => x.Total);
        b.Ignore(x => x.EstModifiable);
        b.Ignore(x => x.BloqueNouvellePrevision);
        b.Ignore(x => x.AttendRapport);
        b.Ignore(x => x.AttendReceptionAdmin);
        b.Ignore(x => x.PlafondDuJour);
        b.Ignore(x => x.Reliquat);
        b.Ignore(x => x.PourcentageDecaisse);
        b.Ignore(x => x.EstAccuseeReception);
        b.Ignore(x => x.AttendAccuseReception);
        b.Ignore(x => x.PeutDecaisser);
    }
}

public class PrevisionLigneConfiguration : IEntityTypeConfiguration<PrevisionLigne>
{
    public void Configure(EntityTypeBuilder<PrevisionLigne> b)
    {
        b.ToTable("PrevisionLignes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Designation).HasMaxLength(150).IsRequired();
        b.Property(x => x.Categorie).HasMaxLength(80).IsRequired();
        b.Property(x => x.TypeBudget).HasConversion<int>();
        b.Property(x => x.Quantite).HasColumnType("numeric(18,3)");
        b.Property(x => x.PrixUnitaireEstime).HasColumnType("numeric(18,2)");
        b.Property(x => x.Observation).HasMaxLength(500);

        b.HasOne(x => x.PrevisionJournaliere).WithMany(p => p.Lignes)
            .HasForeignKey(x => x.PrevisionJournaliereId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Materiau).WithMany()
            .HasForeignKey(x => x.MateriauId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.DetteFournisseur).WithMany()
            .HasForeignKey(x => x.DetteFournisseurId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.PrevisionGlobaleLigne).WithMany()
            .HasForeignKey(x => x.PrevisionGlobaleLigneId).OnDelete(DeleteBehavior.SetNull);

        b.Ignore(x => x.Total);
    }
}
