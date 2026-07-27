using ETAM.Domain.Entities;

namespace ETAM.Domain.Interfaces;

/// <summary>
/// Unit Of Work : expose les repositories et centralise la persistance transactionnelle.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Chantier> Chantiers { get; }
    IGenericRepository<BudgetCompte> BudgetsComptes { get; }
    IGenericRepository<Materiau> Materiaux { get; }
    IGenericRepository<MouvementMateriau> MouvementsMateriau { get; }
    IGenericRepository<PrevisionJournaliere> Previsions { get; }
    IGenericRepository<PrevisionLigne> PrevisionLignes { get; }
    IGenericRepository<Depense> Depenses { get; }
    IGenericRepository<Alerte> Alertes { get; }
    IGenericRepository<AuditLog> AuditLogs { get; }
    IGenericRepository<Parametre> Parametres { get; }

    // --- Trésorerie ---
    IGenericRepository<CompteBancaire> ComptesBancaires { get; }
    IGenericRepository<MouvementBancaire> MouvementsBancaires { get; }
    IGenericRepository<Fournisseur> Fournisseurs { get; }
    IGenericRepository<DetteFournisseur> DettesFournisseurs { get; }

    // --- Approvisionnement ---
    IGenericRepository<Approvisionnement> Approvisionnements { get; }
    IGenericRepository<ApprovisionnementLigne> ApprovisionnementLignes { get; }

    // --- Catalogue ---
    IGenericRepository<ArticleCatalogue> Catalogue { get; }

    // --- Rapports de travail ---
    IGenericRepository<RapportTravail> RapportsTravail { get; }
    IGenericRepository<RapportTravailAvancementLigne> RapportTravailLignesAvancement { get; }
    IGenericRepository<RapportTravailMateriauLigne> RapportTravailLignesMateriaux { get; }
    IGenericRepository<RapportTravailEquipementLigne> RapportTravailLignesEquipements { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
