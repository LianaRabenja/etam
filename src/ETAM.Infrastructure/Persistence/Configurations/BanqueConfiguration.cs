using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class BanqueConfiguration : IEntityTypeConfiguration<Banque>
{
    public void Configure(EntityTypeBuilder<Banque> b)
    {
        b.ToTable("Banques");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nom).HasMaxLength(150).IsRequired();
        b.Property(x => x.Sigle).HasMaxLength(20).IsRequired();
        b.Property(x => x.Agence).HasMaxLength(150);
        b.Property(x => x.Observation).HasMaxLength(500);

        // Deux banques ne peuvent pas porter le même sigle : c'est lui qui sert
        // d'identifiant visuel partout dans l'application.
        b.HasIndex(x => x.Sigle).IsUnique();

        b.Ignore(x => x.Libelle);
    }
}
