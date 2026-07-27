using ETAM.Domain.Entities;
using ETAM.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace ETAM.Infrastructure.Persistence.Repositories;

/// <summary>Unit Of Work : partage le même DbContext entre tous les repositories.</summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Chantiers = new GenericRepository<Chantier>(context);
        BudgetsComptes = new GenericRepository<BudgetCompte>(context);
        Materiaux = new GenericRepository<Materiau>(context);
        MouvementsMateriau = new GenericRepository<MouvementMateriau>(context);
        Previsions = new GenericRepository<PrevisionJournaliere>(context);
        PrevisionLignes = new GenericRepository<PrevisionLigne>(context);
        PrevisionsGlobales = new GenericRepository<PrevisionGlobale>(context);
        PrevisionsGlobalesLignes = new GenericRepository<PrevisionGlobaleLigne>(context);
        Depenses = new GenericRepository<Depense>(context);
        Alertes = new GenericRepository<Alerte>(context);
        AuditLogs = new GenericRepository<AuditLog>(context);
        Parametres = new GenericRepository<Parametre>(context);
        ComptesBancaires = new GenericRepository<CompteBancaire>(context);
        MouvementsBancaires = new GenericRepository<MouvementBancaire>(context);
        Fournisseurs = new GenericRepository<Fournisseur>(context);
        DettesFournisseurs = new GenericRepository<DetteFournisseur>(context);
        Approvisionnements = new GenericRepository<Approvisionnement>(context);
        ApprovisionnementLignes = new GenericRepository<ApprovisionnementLigne>(context);
        Catalogue = new GenericRepository<ArticleCatalogue>(context);
        RapportsTravail = new GenericRepository<RapportTravail>(context);
        RapportTravailLignesAvancement = new GenericRepository<RapportTravailAvancementLigne>(context);
        RapportTravailLignesMateriaux = new GenericRepository<RapportTravailMateriauLigne>(context);
        RapportTravailLignesEquipements = new GenericRepository<RapportTravailEquipementLigne>(context);
    }

    public IGenericRepository<Chantier> Chantiers { get; }
    public IGenericRepository<BudgetCompte> BudgetsComptes { get; }
    public IGenericRepository<Materiau> Materiaux { get; }
    public IGenericRepository<MouvementMateriau> MouvementsMateriau { get; }
    public IGenericRepository<PrevisionJournaliere> Previsions { get; }
    public IGenericRepository<PrevisionLigne> PrevisionLignes { get; }
    public IGenericRepository<PrevisionGlobale> PrevisionsGlobales { get; }
    public IGenericRepository<PrevisionGlobaleLigne> PrevisionsGlobalesLignes { get; }
    public IGenericRepository<Depense> Depenses { get; }
    public IGenericRepository<Alerte> Alertes { get; }
    public IGenericRepository<AuditLog> AuditLogs { get; }
    public IGenericRepository<Parametre> Parametres { get; }
    public IGenericRepository<CompteBancaire> ComptesBancaires { get; }
    public IGenericRepository<MouvementBancaire> MouvementsBancaires { get; }
    public IGenericRepository<Fournisseur> Fournisseurs { get; }
    public IGenericRepository<DetteFournisseur> DettesFournisseurs { get; }
    public IGenericRepository<Approvisionnement> Approvisionnements { get; }
    public IGenericRepository<ApprovisionnementLigne> ApprovisionnementLignes { get; }
    public IGenericRepository<ArticleCatalogue> Catalogue { get; }
    public IGenericRepository<RapportTravail> RapportsTravail { get; }
    public IGenericRepository<RapportTravailAvancementLigne> RapportTravailLignesAvancement { get; }
    public IGenericRepository<RapportTravailMateriauLigne> RapportTravailLignesMateriaux { get; }
    public IGenericRepository<RapportTravailEquipementLigne> RapportTravailLignesEquipements { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_transaction is not null) { await _transaction.CommitAsync(ct); await _transaction.DisposeAsync(); _transaction = null; }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is not null) { await _transaction.RollbackAsync(ct); await _transaction.DisposeAsync(); _transaction = null; }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
