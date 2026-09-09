using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class BudgetCompteConfiguration : IEntityTypeConfiguration<BudgetCompte>
{
    public void Configure(EntityTypeBuilder<BudgetCompte> b)
    {
        b.ToTable("BudgetsComptes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Libelle).HasMaxLength(120).IsRequired();
        b.HasIndex(x => x.Annee).IsUnique();
        b.Property(x => x.MontantInitial).HasColumnType("numeric(18,2)");
        b.Property(x => x.MontantTransfere).HasColumnType("numeric(18,2)");
        b.Property(x => x.MontantConsomme).HasColumnType("numeric(18,2)");
        b.Property(x => x.Reserve).HasColumnType("numeric(18,2)");
        b.Property(x => x.ReserveUtilisee).HasColumnType("numeric(18,2)");

        b.Ignore(x => x.MontantRestant);
        b.Ignore(x => x.PourcentageConsomme);
        b.Ignore(x => x.ReserveRestante);
        b.Ignore(x => x.DisponibleReel);
    }
}
