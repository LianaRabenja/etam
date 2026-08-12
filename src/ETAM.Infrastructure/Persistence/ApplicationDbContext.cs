using System.Reflection;
using ETAM.Domain.Common;
using ETAM.Domain.Entities;
using ETAM.Infrastructure.Identity;
using ETAM.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ETAM.Infrastructure.Persistence;

/// <summary>
/// Contexte EF Core principal. Combine l'identité ASP.NET et les entités métier ETAM,
/// applique les configurations Fluent API, le filtre global de soft-delete et
/// l'interception d'audit.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly AuditableEntityInterceptor _auditInterceptor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        AuditableEntityInterceptor auditInterceptor) : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    public DbSet<Chantier> Chantiers => Set<Chantier>();
    public DbSet<BudgetCompte> BudgetsComptes => Set<BudgetCompte>();
    public DbSet<Materiau> Materiaux => Set<Materiau>();
    public DbSet<MouvementMateriau> MouvementsMateriau => Set<MouvementMateriau>();
    public DbSet<PrevisionJournaliere> Previsions => Set<PrevisionJournaliere>();
    public DbSet<PrevisionLigne> PrevisionLignes => Set<PrevisionLigne>();
    public DbSet<PrevisionMensuelle> PrevisionsMensuelles => Set<PrevisionMensuelle>();
    public DbSet<PrevisionMensuelleLigne> PrevisionMensuelleLignes => Set<PrevisionMensuelleLigne>();
    public DbSet<PlanJournalier> PlansJournaliers => Set<PlanJournalier>();
    public DbSet<Decaissement> Decaissements => Set<Decaissement>();
    public DbSet<PieceJointe> PiecesJointes => Set<PieceJointe>();
    public DbSet<PrevisionGlobale> PrevisionsGlobales => Set<PrevisionGlobale>();
    public DbSet<PrevisionGlobaleLigne> PrevisionsGlobalesLignes => Set<PrevisionGlobaleLigne>();
    public DbSet<Depense> Depenses => Set<Depense>();
    public DbSet<Alerte> Alertes => Set<Alerte>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Parametre> Parametres => Set<Parametre>();
    public DbSet<CompteBancaire> ComptesBancaires => Set<CompteBancaire>();
    public DbSet<MouvementBancaire> MouvementsBancaires => Set<MouvementBancaire>();
    public DbSet<Fournisseur> Fournisseurs => Set<Fournisseur>();
    public DbSet<DetteFournisseur> DettesFournisseurs => Set<DetteFournisseur>();
    public DbSet<Approvisionnement> Approvisionnements => Set<Approvisionnement>();
    public DbSet<ApprovisionnementLigne> ApprovisionnementLignes => Set<ApprovisionnementLigne>();
    public DbSet<ArticleCatalogue> Catalogue => Set<ArticleCatalogue>();
    public DbSet<RapportTravail> RapportsTravail => Set<RapportTravail>();
    public DbSet<RapportTravailAvancementLigne> RapportTravailLignesAvancement => Set<RapportTravailAvancementLigne>();
    public DbSet<RapportTravailMateriauLigne> RapportTravailLignesMateriaux => Set<RapportTravailMateriauLigne>();
    public DbSet<RapportTravailEquipementLigne> RapportTravailLignesEquipements => Set<RapportTravailEquipementLigne>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.AddInterceptors(_auditInterceptor);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Applique toutes les IEntityTypeConfiguration de l'assembly.
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Filtre global de soft-delete sur toutes les entités métier.
        builder.Entity<Chantier>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<BudgetCompte>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Materiau>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<MouvementMateriau>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<PrevisionJournaliere>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<PrevisionLigne>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<PrevisionMensuelle>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<PrevisionMensuelleLigne>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<PlanJournalier>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Decaissement>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<PieceJointe>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<PrevisionGlobale>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<PrevisionGlobaleLigne>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Depense>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Alerte>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<AuditLog>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Parametre>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<CompteBancaire>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<MouvementBancaire>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Fournisseur>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<DetteFournisseur>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Approvisionnement>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ApprovisionnementLigne>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ArticleCatalogue>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<RapportTravail>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<RapportTravailAvancementLigne>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<RapportTravailMateriauLigne>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<RapportTravailEquipementLigne>().HasQueryFilter(e => !e.IsDeleted);

        // PostgreSQL (timestamptz) n'accepte que des DateTime en UTC.
        // On force globalement Kind=Utc à l'écriture et à la lecture pour toutes
        // les propriétés DateTime / DateTime?, ce qui évite les erreurs
        // « Cannot write DateTime with Kind=Unspecified ».
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var utcNullableConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(utcConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(utcNullableConverter);
            }
        }

        // Mappe le xmin PostgreSQL sur RowVersion (concurrence optimiste).
        foreach (var entity in builder.Model.GetEntityTypes()
                     .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            builder.Entity(entity.ClrType).Property(nameof(BaseEntity.RowVersion))
                .HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();
        }
    }
}
