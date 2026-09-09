using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class DepenseConfiguration : IEntityTypeConfiguration<Depense>
{
    public void Configure(EntityTypeBuilder<Depense> b)
    {
        b.ToTable("Depenses");
        b.HasKey(x => x.Id);
        b.Property(x => x.Categorie).HasMaxLength(80).IsRequired();
        b.Property(x => x.Designation).HasMaxLength(150).IsRequired();
        b.Property(x => x.Quantite).HasColumnType("numeric(18,3)");
        b.Property(x => x.PrixUnitaire).HasColumnType("numeric(18,2)");
        b.Property(x => x.BudgetConcerne).HasConversion<int>();
        b.Property(x => x.Justificatif).HasMaxLength(250);
        b.Property(x => x.Observation).HasMaxLength(500);

        b.HasOne(x => x.Chantier).WithMany(c => c.Depenses)
            .HasForeignKey(x => x.ChantierId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PrevisionJournaliere).WithMany()
            .HasForeignKey(x => x.PrevisionJournaliereId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => x.Date);

        b.Ignore(x => x.Montant);
    }
}
