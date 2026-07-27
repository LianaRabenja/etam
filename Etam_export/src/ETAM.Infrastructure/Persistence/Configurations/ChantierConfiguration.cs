using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class ChantierConfiguration : IEntityTypeConfiguration<Chantier>
{
    public void Configure(EntityTypeBuilder<Chantier> b)
    {
        b.ToTable("Chantiers");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nom).HasMaxLength(150).IsRequired();
        b.Property(x => x.Code).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Localisation).HasMaxLength(150);
        b.Property(x => x.Responsable).HasMaxLength(120);
        b.Property(x => x.Observation).HasMaxLength(1000);

        b.Property(x => x.BudgetMateriel).HasColumnType("numeric(18,2)");
        b.Property(x => x.Reserve).HasColumnType("numeric(18,2)");
        b.Property(x => x.ReserveUtilisee).HasColumnType("numeric(18,2)");
        b.Property(x => x.Consommation).HasColumnType("numeric(18,2)");
        b.Property(x => x.MaterielTransfere).HasColumnType("numeric(18,2)");

        // Propriétés calculées : non persistées.
        b.Ignore(x => x.BudgetMaterielRestant);
        b.Ignore(x => x.ReserveRestante);
        b.Ignore(x => x.PourcentageConsomme);
        b.Ignore(x => x.MaterielDisponible);
    }
}
