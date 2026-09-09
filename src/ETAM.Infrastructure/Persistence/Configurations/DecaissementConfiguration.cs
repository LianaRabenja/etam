using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class DecaissementConfiguration : IEntityTypeConfiguration<Decaissement>
{
    public void Configure(EntityTypeBuilder<Decaissement> b)
    {
        b.ToTable("Decaissements");
        b.HasKey(x => x.Id);

        b.Property(x => x.Beneficiaire).HasMaxLength(150).IsRequired();
        b.Property(x => x.Motif).HasMaxLength(300).IsRequired();
        b.Property(x => x.Montant).HasColumnType("numeric(18,2)");
        b.Property(x => x.Mode).HasConversion<int>();
        b.Property(x => x.BudgetConcerne).HasConversion<int>();
        b.Property(x => x.Reference).HasMaxLength(80);
        b.Property(x => x.AccuseNom).HasMaxLength(150);
        b.Property(x => x.Observation).HasMaxLength(500);

        b.HasOne(x => x.PrevisionJournaliere).WithMany(p => p.Decaissements)
            .HasForeignKey(x => x.PrevisionJournaliereId).OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.PrevisionLigne).WithMany()
            .HasForeignKey(x => x.PrevisionLigneId).OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.CompteBancaire).WithMany()
            .HasForeignKey(x => x.CompteBancaireId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.PrevisionJournaliereId, x.Date });
        b.HasIndex(x => x.Date);

        b.Ignore(x => x.EstAccuse);
    }
}

public class PieceJointeConfiguration : IEntityTypeConfiguration<PieceJointe>
{
    public void Configure(EntityTypeBuilder<PieceJointe> b)
    {
        b.ToTable("PiecesJointes");
        b.HasKey(x => x.Id);

        b.Property(x => x.NomFichier).HasMaxLength(255).IsRequired();
        b.Property(x => x.TypeMime).HasMaxLength(100).IsRequired();
        b.Property(x => x.Description).HasMaxLength(300);
        b.Property(x => x.NumeroPiece).HasMaxLength(80);
        b.Property(x => x.Emetteur).HasMaxLength(150);
        b.Property(x => x.MontantFacture).HasColumnType("numeric(18,2)");

        // bytea PostgreSQL. Non chargé par défaut dans les listes : les requêtes
        // d'affichage doivent projeter sans la colonne Contenu.
        b.Property(x => x.Contenu).HasColumnType("bytea").IsRequired();

        b.HasOne(x => x.PrevisionJournaliere).WithMany(p => p.PiecesJointes)
            .HasForeignKey(x => x.PrevisionJournaliereId).OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Decaissement).WithMany(d => d.PiecesJointes)
            .HasForeignKey(x => x.DecaissementId).OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.RapportTravail).WithMany()
            .HasForeignKey(x => x.RapportTravailId).OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.PrevisionJournaliereId);
        b.HasIndex(x => x.DecaissementId);
        b.HasIndex(x => x.RapportTravailId);

        b.Ignore(x => x.EstImage);
        b.Ignore(x => x.TailleLisible);
    }
}
