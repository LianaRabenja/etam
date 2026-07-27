using ETAM.Application.Common.Models;
using ETAM.Application.DTOs;
using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ETAM.Application.Services;

/// <summary>
/// Cœur métier de l'ERP : gestion du cycle de vie et de l'exécution des prévisions
/// journalières, avec impact transactionnel sur les budgets et les stocks.
/// </summary>
public class PrevisionService : IPrevisionService
{
    private readonly IUnitOfWork _uow;
    private readonly IBudgetService _budgetService;
    private readonly IAuditService _audit;
    private readonly IAlerteService _alertes;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<PrevisionService> _logger;

    public PrevisionService(
        IUnitOfWork uow,
        IBudgetService budgetService,
        IAuditService audit,
        IAlerteService alertes,
        ICurrentUserService currentUser,
        ILogger<PrevisionService> logger)
    {
        _uow = uow;
        _budgetService = budgetService;
        _audit = audit;
        _alertes = alertes;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<long>> CreerAsync(PrevisionCreateDto dto, CancellationToken ct = default)
    {
        var chantier = await _uow.Chantiers.GetByIdAsync(dto.ChantierId, ct);
        if (chantier is null) return Result<long>.Failure("Chantier introuvable.");

        var prevision = new PrevisionJournaliere
        {
            ChantierId = dto.ChantierId,
            DatePrevision = dto.DatePrevision,
            Reference = $"PREV-{chantier.Code}-{dto.DatePrevision:yyyyMMdd}-{DateTime.UtcNow.Ticks % 10000:D4}",
            Statut = StatutPrevision.Brouillon,
            Observation = dto.Observation,
            Lignes = dto.Lignes.Select(l => new PrevisionLigne
            {
                Designation = l.Designation,
                Categorie = l.Categorie,
                TypeBudget = l.TypeBudget,
                MateriauId = l.MateriauId,
                DetteFournisseurId = l.DetteFournisseurId,
                PrevisionGlobaleLigneId = l.PrevisionGlobaleLigneId,
                Quantite = l.Quantite,
                PrixUnitaireEstime = l.PrixUnitaireEstime,
                Observation = l.Observation
            }).ToList()
        };

        await _uow.Previsions.AddAsync(prevision, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync(TypeActionAudit.Ajout, nameof(PrevisionJournaliere), prevision.Id.ToString(), ct: ct);

        return Result<long>.Success(prevision.Id);
    }

    public async Task<Result> SoumettreAsync(long id, CancellationToken ct = default)
    {
        var p = await ChargerAsync(id, ct);
        if (p is null) return Result.Failure("Prévision introuvable.");
        if (p.Statut != StatutPrevision.Brouillon)
            return Result.Failure("Seule une prévision en brouillon peut être soumise.");

        p.Statut = StatutPrevision.Soumise;
        p.SoumisePar = _currentUser.UserId;
        p.DateSoumission = DateTime.UtcNow;
        _uow.Previsions.Update(p);
        await _uow.SaveChangesAsync(ct);

        await _alertes.CreerAsync(TypeAlerte.ValidationEnAttente, NiveauAlerte.Info,
            "Prévision à valider", $"La prévision {p.Reference} attend la validation du Responsable Financier.", p.ChantierId, ct);
        await _audit.LogAsync(TypeActionAudit.Validation, nameof(PrevisionJournaliere), id.ToString(), ct: ct);
        return Result.Success();
    }

    public async Task<Result> ValiderResponsableFinancierAsync(long id, CancellationToken ct = default)
    {
        var p = await ChargerAsync(id, ct);
        if (p is null) return Result.Failure("Prévision introuvable.");
        if (p.Statut != StatutPrevision.Soumise)
            return Result.Failure("La prévision doit être 'Soumise' pour être validée par le Responsable Financier.");

        p.Statut = StatutPrevision.ValideeResponsableFinancier;
        p.ValideeParRfId = _currentUser.UserId;
        p.DateValidationRf = DateTime.UtcNow;
        _uow.Previsions.Update(p);
        await _uow.SaveChangesAsync(ct);

        await _alertes.CreerAsync(TypeAlerte.ValidationEnAttente, NiveauAlerte.Avertissement,
            "Validation Administrateur", $"La prévision {p.Reference} attend la validation de l'Administrateur.", p.ChantierId, ct);
        await _audit.LogAsync(TypeActionAudit.Validation, nameof(PrevisionJournaliere), id.ToString(),
            nouvelleValeur: "ValideeResponsableFinancier", ct: ct);
        return Result.Success();
    }

    public async Task<Result> ValiderAdministrateurAsync(long id, CancellationToken ct = default)
    {
        var p = await ChargerAsync(id, ct);
        if (p is null) return Result.Failure("Prévision introuvable.");
        if (p.Statut != StatutPrevision.ValideeResponsableFinancier)
            return Result.Failure("La prévision doit d'abord être validée par le Responsable Financier.");

        p.Statut = StatutPrevision.ValideeAdministrateur;
        p.ValideeParAdminId = _currentUser.UserId;
        p.DateValidationAdmin = DateTime.UtcNow;
        _uow.Previsions.Update(p);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync(TypeActionAudit.Validation, nameof(PrevisionJournaliere), id.ToString(),
            nouvelleValeur: "ValideeAdministrateur", ct: ct);
        return Result.Success();
    }

    public async Task<Result> RefuserAsync(long id, string motif, CancellationToken ct = default)
    {
        var p = await ChargerAsync(id, ct);
        if (p is null) return Result.Failure("Prévision introuvable.");
        if (p.Statut is StatutPrevision.Executee or StatutPrevision.Refusee)
            return Result.Failure("Cette prévision ne peut plus être refusée.");

        p.Statut = StatutPrevision.Refusee;
        p.MotifRefus = motif;
        _uow.Previsions.Update(p);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync(TypeActionAudit.Refus, nameof(PrevisionJournaliere), id.ToString(), nouvelleValeur: motif, ct: ct);
        return Result.Success();
    }

    /// <summary>
    /// Exécution transactionnelle : impacte les budgets et les stocks.
    /// </summary>
    public async Task<Result> ExecuterAsync(long id, bool utiliserReserve = false, CancellationToken ct = default)
    {
        var p = await ChargerAsync(id, ct);
        if (p is null) return Result.Failure("Prévision introuvable.");
        if (p.Statut != StatutPrevision.ValideeAdministrateur)
            return Result.Failure("Seule une prévision validée par l'Administrateur peut être exécutée.");

        var chantier = await _uow.Chantiers.GetByIdAsync(p.ChantierId, ct);
        if (chantier is null) return Result.Failure("Chantier introuvable.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            foreach (var ligne in p.Lignes.Where(l => !l.IsDeleted))
            {
                if (ligne.TypeBudget == TypeBudget.Compte)
                {
                    var res = await _budgetService.ImputerBudgetCompteAsync(ligne.Total, utiliserReserve, ct);
                    if (!res.Succeeded) { await _uow.RollbackAsync(ct); return res; }

                    // Remboursement de dette : si la ligne cible une dette, elle diminue.
                    if (ligne.DetteFournisseurId.HasValue)
                    {
                        var dette = await _uow.DettesFournisseurs.GetByIdAsync(ligne.DetteFournisseurId.Value, ct);
                        if (dette is not null)
                        {
                            dette.MontantPaye += ligne.Total;
                            dette.Statut = dette.SoldeRestant <= 0
                                ? StatutDette.Soldee
                                : StatutDette.PartiellementPayee;
                            _uow.DettesFournisseurs.Update(dette);
                        }
                    }
                }
                else // Matériel
                {
                    var depassement = ligne.Total - chantier.BudgetMaterielRestant;
                    if (depassement > 0)
                    {
                        if (!utiliserReserve)
                        {
                            await _uow.RollbackAsync(ct);
                            return Result.Failure(
                                $"Budget Matériel du chantier {chantier.Nom} dépassé de {depassement:N0} Ar. Réserve requise.");
                        }
                        if (depassement > chantier.ReserveRestante)
                        {
                            await _uow.RollbackAsync(ct);
                            return Result.Failure($"Réserve du chantier insuffisante ({chantier.ReserveRestante:N0} Ar).");
                        }
                        chantier.ReserveUtilisee += depassement;
                        chantier.Consommation = chantier.BudgetMateriel;
                    }
                    else
                    {
                        chantier.Consommation += ligne.Total;
                    }

                    // Diminution automatique du stock si un matériau est ciblé.
                    if (ligne.MateriauId.HasValue)
                    {
                        var mat = await _uow.Materiaux.GetByIdAsync(ligne.MateriauId.Value, ct);
                        if (mat is not null)
                        {
                            mat.QuantiteUtilisee += ligne.Quantite;
                            _uow.Materiaux.Update(mat);
                        }
                    }
                    _uow.Chantiers.Update(chantier);
                }

                // Génère la dépense réelle correspondante.
                await _uow.Depenses.AddAsync(new Depense
                {
                    Date = DateTime.UtcNow,
                    ChantierId = p.ChantierId,
                    PrevisionJournaliereId = p.Id,
                    Categorie = ligne.Categorie,
                    Designation = ligne.Designation,
                    Quantite = ligne.Quantite,
                    PrixUnitaire = ligne.PrixUnitaireEstime,
                    BudgetConcerne = ligne.TypeBudget
                }, ct);
            }

            p.Statut = StatutPrevision.Executee;
            p.DateExecution = DateTime.UtcNow;
            _uow.Previsions.Update(p);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            await _alertes.EvaluerAlertesAsync(ct);
            await _audit.LogAsync(TypeActionAudit.Execution, nameof(PrevisionJournaliere), id.ToString(), ct: ct);
            _logger.LogInformation("Prévision {Ref} exécutée (chantier {Chantier}).", p.Reference, chantier.Nom);
            return Result.Success();
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync(ct);
            _logger.LogError(ex, "Échec d'exécution de la prévision {Id}.", id);
            return Result.Failure("Une erreur est survenue lors de l'exécution : " + ex.Message);
        }
    }

    private async Task<PrevisionJournaliere?> ChargerAsync(long id, CancellationToken ct)
    {
        return await _uow.Previsions.Query()
            .Include(p => p.Lignes)
            .Include(p => p.Chantier)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
    }
}
