using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class CompteBancaireConfiguration : IEntityTypeConfiguration<CompteBancaire>
{
    public void Configure(EntityTypeBuilder<CompteBancaire> b)
    {
        b.ToTable("ComptesBancaires");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nom).HasMaxLength(120).IsRequired();
        b.Property(x => x.Banque).HasMaxLength(80).IsRequired();
        b.Property(x => x.Numero).HasMaxLength(60);
        b.Property(x => x.Devise).HasMaxLength(10);
        b.Property(x => x.Solde).HasColumnType("numeric(18,2)");
        b.Property(x => x.Type).HasConversion<int>();
        b.HasOne(x => x.Chantier).WithMany()
            .HasForeignKey(x => x.ChantierId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => x.ChantierId);
    }
}

public class MouvementBancaireConfiguration : IEntityTypeConfiguration<MouvementBancaire>
{
    public void Configure(EntityTypeBuilder<MouvementBancaire> b)
    {
        b.ToTable("MouvementsBancaires");
        b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasConversion<int>();
        b.Property(x => x.Montant).HasColumnType("numeric(18,2)");
        b.Property(x => x.Beneficiaire).HasMaxLength(150);
        b.Property(x => x.Motif).HasMaxLength(300);
        b.Property(x => x.Reference).HasMaxLength(60);
        b.Property(x => x.DemandePar).HasMaxLength(150);
        b.Property(x => x.EstValide).HasDefaultValue(true);

        b.HasOne(x => x.CompteBancaire).WithMany(c => c.Mouvements)
            .HasForeignKey(x => x.CompteBancaireId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Chantier).WithMany()
            .HasForeignKey(x => x.ChantierId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Fournisseur).WithMany()
            .HasForeignKey(x => x.FournisseurId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.DetteFournisseur).WithMany()
            .HasForeignKey(x => x.DetteFournisseurId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => new { x.CompteBancaireId, x.Date });

        b.Ignore(x => x.Sens);
        b.Ignore(x => x.MontantSigne);
    }
}

public class FournisseurConfiguration : IEntityTypeConfiguration<Fournisseur>
{
    public void Configure(EntityTypeBuilder<Fournisseur> b)
    {
        b.ToTable("Fournisseurs");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nom).HasMaxLength(150).IsRequired();
        b.Property(x => x.Contact).HasMaxLength(120);
        b.Property(x => x.Telephone).HasMaxLength(40);
        b.Property(x => x.Adresse).HasMaxLength(250);
        b.Property(x => x.Nif).HasMaxLength(40);
        b.HasIndex(x => x.Nom);
    }
}

public class DetteFournisseurConfiguration : IEntityTypeConfiguration<DetteFournisseur>
{
    public void Configure(EntityTypeBuilder<DetteFournisseur> b)
    {
        b.ToTable("DettesFournisseurs");
        b.HasKey(x => x.Id);
        b.Property(x => x.Libelle).HasMaxLength(200).IsRequired();
        b.Property(x => x.Statut).HasConversion<int>();
        b.Property(x => x.MontantInitial).HasColumnType("numeric(18,2)");
        b.Property(x => x.MontantPaye).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Fournisseur).WithMany(f => f.Dettes)
            .HasForeignKey(x => x.FournisseurId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Chantier).WithMany()
            .HasForeignKey(x => x.ChantierId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => x.Statut);

        b.Ignore(x => x.SoldeRestant);
        b.Ignore(x => x.PourcentagePaye);
    }
}
