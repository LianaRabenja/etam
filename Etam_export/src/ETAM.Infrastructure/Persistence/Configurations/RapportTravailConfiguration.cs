using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class RapportTravailConfiguration : IEntityTypeConfiguration<RapportTravail>
{
    public void Configure(EntityTypeBuilder<RapportTravail> builder)
    {
        builder.Property(r => r.Numero).IsRequired().HasMaxLength(20);
        builder.Property(r => r.Lieu).HasMaxLength(200);
        builder.Property(r => r.EntrepriseExecutante).HasMaxLength(150);
        builder.Property(r => r.ConducteurTravaux).HasMaxLength(150);
        builder.Property(r => r.HoraireMatin).HasMaxLength(50);
        builder.Property(r => r.HoraireApresMidi).HasMaxLength(50);
        builder.Property(r => r.ConditionsMeteo).HasMaxLength(300);
        builder.Property(r => r.ResumeSuiviPlanning).HasColumnType("text");
        builder.Property(r => r.ProblemesRencontres).HasColumnType("text");
        builder.Property(r => r.Suggestions).HasColumnType("text");
        builder.Property(r => r.MotifRefus).HasMaxLength(500);

        builder.HasOne(r => r.Chantier).WithMany().HasForeignKey(r => r.ChantierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(r => new { r.ChantierId, r.PeriodeFin });
        builder.HasIndex(r => r.Statut);

        builder.HasMany(r => r.LignesAvancement).WithOne(l => l.RapportTravail)
            .HasForeignKey(l => l.RapportTravailId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(r => r.LignesMateriaux).WithOne(l => l.RapportTravail)
            .HasForeignKey(l => l.RapportTravailId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(r => r.LignesEquipements).WithOne(l => l.RapportTravail)
            .HasForeignKey(l => l.RapportTravailId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class RapportTravailAvancementLigneConfiguration : IEntityTypeConfiguration<RapportTravailAvancementLigne>
{
    public void Configure(EntityTypeBuilder<RapportTravailAvancementLigne> builder)
    {
        builder.Property(l => l.Zone).IsRequired().HasMaxLength(150);
        builder.Property(l => l.TravauxRealises).IsRequired().HasColumnType("text");
        builder.Property(l => l.NiveauAvancement).IsRequired().HasMaxLength(150);
        builder.Property(l => l.Observations).HasColumnType("text");
    }
}

public class RapportTravailMateriauLigneConfiguration : IEntityTypeConfiguration<RapportTravailMateriauLigne>
{
    public void Configure(EntityTypeBuilder<RapportTravailMateriauLigne> builder)
    {
        builder.Property(l => l.Materiau).IsRequired().HasMaxLength(150);
        builder.Property(l => l.Unite).HasMaxLength(20);
        builder.Property(l => l.QuantiteUtilisee).HasColumnType("numeric(18,3)");
        builder.Property(l => l.StockInitial).HasColumnType("numeric(18,3)");
        builder.Property(l => l.Entree).HasColumnType("numeric(18,3)");
        builder.Property(l => l.StockRestant).HasColumnType("numeric(18,3)");
        builder.Property(l => l.Observations).HasColumnType("text");
    }
}

public class RapportTravailEquipementLigneConfiguration : IEntityTypeConfiguration<RapportTravailEquipementLigne>
{
    public void Configure(EntityTypeBuilder<RapportTravailEquipementLigne> builder)
    {
        builder.Property(l => l.Equipement).IsRequired().HasMaxLength(150);
        builder.Property(l => l.Etat).HasMaxLength(80);
        builder.Property(l => l.Observation).HasColumnType("text");
    }
}
