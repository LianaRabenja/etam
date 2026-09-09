using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class AutreDepenseJourConfiguration : IEntityTypeConfiguration<AutreDepenseJour>
{
    public void Configure(EntityTypeBuilder<AutreDepenseJour> b)
    {
        b.ToTable("AutresDepensesJour");
        b.HasKey(x => x.Id);

        b.Property(x => x.Libelle).HasMaxLength(200).IsRequired();
        b.Property(x => x.Montant).HasColumnType("numeric(18,2)");
        b.Property(x => x.Observation).HasMaxLength(300);

        b.HasOne(x => x.Chantier).WithMany()
            .HasForeignKey(x => x.ChantierId).OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(x => x.Date);
    }
}
