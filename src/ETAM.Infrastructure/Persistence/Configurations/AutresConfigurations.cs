using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class AlerteConfiguration : IEntityTypeConfiguration<Alerte>
{
    public void Configure(EntityTypeBuilder<Alerte> b)
    {
        b.ToTable("Alertes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Titre).HasMaxLength(150).IsRequired();
        b.Property(x => x.Message).HasMaxLength(1000).IsRequired();
        b.Property(x => x.Type).HasConversion<int>();
        b.Property(x => x.Niveau).HasConversion<int>();
        b.HasOne(x => x.Chantier).WithMany()
            .HasForeignKey(x => x.ChantierId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => new { x.EstLue, x.CreatedAt });
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs");
        b.HasKey(x => x.Id);
        b.Property(x => x.Action).HasConversion<int>();
        b.Property(x => x.Entite).HasMaxLength(100);
        b.Property(x => x.CleEntite).HasMaxLength(60);
        b.Property(x => x.UtilisateurId).HasMaxLength(450);
        b.Property(x => x.UtilisateurNom).HasMaxLength(150);
        b.Property(x => x.AdresseIp).HasMaxLength(60);
        b.Property(x => x.Navigateur).HasMaxLength(300);
        b.HasIndex(x => x.DateAction);
    }
}

public class ParametreConfiguration : IEntityTypeConfiguration<Parametre>
{
    public void Configure(EntityTypeBuilder<Parametre> b)
    {
        b.ToTable("Parametres");
        b.HasKey(x => x.Id);
        b.Property(x => x.Cle).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Cle).IsUnique();
        b.Property(x => x.Valeur).HasMaxLength(1000);
        b.Property(x => x.Groupe).HasMaxLength(60);
        b.Property(x => x.Description).HasMaxLength(300);
    }
}
