using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class MouvementMateriauConfiguration : IEntityTypeConfiguration<MouvementMateriau>
{
    public void Configure(EntityTypeBuilder<MouvementMateriau> b)
    {
        b.ToTable("MouvementsMateriau");
        b.HasKey(x => x.Id);

        b.Property(x => x.DateMouvement).IsRequired();
        b.Property(x => x.BesoinOuObjectif).HasMaxLength(150);
        b.Property(x => x.QuantiteEntree).HasColumnType("numeric(18,3)").HasDefaultValue(0);
        b.Property(x => x.QuantiteSortie).HasColumnType("numeric(18,3)").HasDefaultValue(0);
        b.Property(x => x.Motif).HasMaxLength(100);
        b.Property(x => x.SoldeSurBesoin).HasColumnType("numeric(18,3)").HasDefaultValue(0);
        b.Property(x => x.SoldeEnStock).HasColumnType("numeric(18,3)").HasDefaultValue(0);

        b.HasOne(x => x.Materiau).WithMany()
            .HasForeignKey(x => x.MateriauxId).OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.MateriauxId, x.DateMouvement }).IsDescending(false, true);
    }
}
