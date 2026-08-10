using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class PrevisionMensuelleConfiguration : IEntityTypeConfiguration<PrevisionMensuelle>
{
    public void Configure(EntityTypeBuilder<PrevisionMensuelle> b)
    {
        b.ToTable("PrevisionsMensuelles");
        b.HasKey(x => x.Id);

        b.Property(x => x.Reference).HasMaxLength(60).IsRequired();
        b.HasIndex(x => x.Reference).IsUnique();

        b.Property(x => x.MontantPrevu).HasColumnType("numeric(18,2)");
        b.Property(x => x.ReportMoisPrecedent).HasColumnType("numeric(18,2)");
        b.Property(x => x.MontantConsomme).HasColumnType("numeric(18,2)");

        b.Property(x => x.Statut).HasConversion<int>();
        b.Property(x => x.Observation).HasMaxLength(1000);
        b.Property(x => x.MotifRefus).HasMaxLength(500);

        b.HasOne(x => x.Chantier).WithMany(c => c.PrevisionsMensuelles)
            .HasForeignKey(x => x.ChantierId).OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.PrevisionGlobale).WithMany()
            .HasForeignKey(x => x.PrevisionGlobaleId).OnDelete(DeleteBehavior.SetNull);

        // Chaînage des mois : le report d'août pointe vers juillet.
        b.HasOne(x => x.PrevisionMensuellePrecedente).WithMany()
            .HasForeignKey(x => x.PrevisionMensuellePrecedenteId).OnDelete(DeleteBehavior.Restrict);

        // Un seul mois par chantier et par période.
        b.HasIndex(x => new { x.ChantierId, x.Annee, x.Mois }).IsUnique();

        b.Ignore(x => x.EnveloppeTotale);
        b.Ignore(x => x.Disponible);
        b.Ignore(x => x.PourcentageConsomme);
        b.Ignore(x => x.TotalLignes);
        b.Ignore(x => x.EstOuvert);
        b.Ignore(x => x.EstModifiable);
        b.Ignore(x => x.Libelle);
    }
}

public class PrevisionMensuelleLigneConfiguration : IEntityTypeConfiguration<PrevisionMensuelleLigne>
{
    public void Configure(EntityTypeBuilder<PrevisionMensuelleLigne> b)
    {
        b.ToTable("PrevisionMensuelleLignes");
        b.HasKey(x => x.Id);

        b.Property(x => x.Rubrique).HasMaxLength(80).IsRequired();
        b.Property(x => x.Designation).HasMaxLength(150);
        b.Property(x => x.Montant).HasColumnType("numeric(18,2)");
        b.Property(x => x.Observation).HasMaxLength(500);

        b.HasOne(x => x.PrevisionMensuelle).WithMany(p => p.Lignes)
            .HasForeignKey(x => x.PrevisionMensuelleId).OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.PrevisionGlobaleLigne).WithMany()
            .HasForeignKey(x => x.PrevisionGlobaleLigneId).OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(x => new { x.PrevisionMensuelleId, x.Rubrique });
    }
}
