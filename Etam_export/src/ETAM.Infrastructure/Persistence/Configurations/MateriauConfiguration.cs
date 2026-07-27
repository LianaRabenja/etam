using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class MateriauConfiguration : IEntityTypeConfiguration<Materiau>
{
    public void Configure(EntityTypeBuilder<Materiau> b)
    {
        b.ToTable("Materiaux");
        b.HasKey(x => x.Id);
        b.Property(x => x.Categorie).HasMaxLength(80).IsRequired();
        b.Property(x => x.Designation).HasMaxLength(150).IsRequired();
        b.Property(x => x.Unite).HasMaxLength(20).IsRequired();

        b.Property(x => x.QuantiteCommandee).HasColumnType("numeric(18,3)");
        b.Property(x => x.QuantiteRecue).HasColumnType("numeric(18,3)");
        b.Property(x => x.QuantiteUtilisee).HasColumnType("numeric(18,3)");
        b.Property(x => x.SeuilMinimal).HasColumnType("numeric(18,3)");
        b.Property(x => x.PrixUnitaire).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Chantier).WithMany(c => c.Materiaux)
            .HasForeignKey(x => x.ChantierId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.ChantierId, x.Designation });

        b.Ignore(x => x.StockDisponible);
        b.Ignore(x => x.ValeurRestante);
        b.Ignore(x => x.PourcentageReception);
        b.Ignore(x => x.EstStockFaible);
        b.Ignore(x => x.EstStockCritique);
    }
}
