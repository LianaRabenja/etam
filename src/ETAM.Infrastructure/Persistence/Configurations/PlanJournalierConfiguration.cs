using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class PlanJournalierConfiguration : IEntityTypeConfiguration<PlanJournalier>
{
    public void Configure(EntityTypeBuilder<PlanJournalier> b)
    {
        b.ToTable("PlansJournaliers");
        b.HasKey(x => x.Id);

        b.Property(x => x.MontantPrevu).HasColumnType("numeric(18,2)");
        b.Property(x => x.Observation).HasMaxLength(300);

        b.HasOne(x => x.PrevisionMensuelle).WithMany()
            .HasForeignKey(x => x.PrevisionMensuelleId).OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Chantier).WithMany()
            .HasForeignKey(x => x.ChantierId).OnDelete(DeleteBehavior.Restrict);

        // Un seul plan par chantier et par jour.
        b.HasIndex(x => new { x.ChantierId, x.Date }).IsUnique();
        b.HasIndex(x => x.PrevisionMensuelleId);

        b.Ignore(x => x.NumeroSemaine);
        b.Ignore(x => x.Libelle);
    }
}
