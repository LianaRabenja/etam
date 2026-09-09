using ETAM.Application.Common.Models;
using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ETAM.Application.Services;

/// <summary>Implémentation de la gestion de trésorerie.</summary>
public class BanqueService : IBanqueService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;
    private readonly ILogger<BanqueService> _logger;

    public BanqueService(IUnitOfWork uow, IAuditService audit, ILogger<BanqueService> logger)
    {
        _uow = uow;
        _audit = audit;
        _logger = logger;
    }

    public async Task<Result> EnregistrerMouvementAsync(
        long compteId, TypeMouvementBancaire type, decimal montant,
        string? beneficiaire, string? motif, string? reference, long? chantierId,
        CancellationToken ct = default)
    {
        if (montant <= 0) return Result.Failure("Le montant doit être positif.");

        var compte = await _uow.ComptesBancaires.GetByIdAsync(compteId, ct);
        if (compte is null) return Result.Failure("Compte bancaire introuvable.");

        // Débit : dépôt exclu.
        var estDebit = type != TypeMouvementBancaire.Depot;
        if (estDebit && montant > compte.Solde)
            return Result.Failure($"Solde insuffisant ({compte.Solde:N0} Ar) pour ce {type.ToString().ToLower()}.");

        await _uow.MouvementsBancaires.AddAsync(new MouvementBancaire
        {
            CompteBancaireId = compteId,
            Type = type,
            Montant = montant,
            Beneficiaire = beneficiaire,
            Motif = motif,
            Reference = reference,
            ChantierId = chantierId,
            Date = DateTime.UtcNow
        }, ct);

        compte.Solde += estDebit ? -montant : montant;
        _uow.ComptesBancaires.Update(compte);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(TypeActionAudit.Ajout, nameof(MouvementBancaire), compteId.ToString(),
            nouvelleValeur: $"{type} {montant:N0} Ar", ct: ct);
        return Result.Success();
    }

    public async Task<Result> PayerDetteAsync(
        long detteId, long compteId, decimal montant, bool genererDepense,
        CancellationToken ct = default)
    {
        if (montant <= 0) return Result.Failure("Le montant doit être positif.");

        var dette = await _uow.DettesFournisseurs.GetByIdAsync(detteId, ct);
        if (dette is null) return Result.Failure("Dette introuvable.");

        var compte = await _uow.ComptesBancaires.GetByIdAsync(compteId, ct);
        if (compte is null) return Result.Failure("Compte bancaire introuvable.");

        if (montant > dette.SoldeRestant)
            return Result.Failure($"Le montant dépasse le solde restant dû ({dette.SoldeRestant:N0} Ar).");
        if (montant > compte.Solde)
            return Result.Failure($"Solde bancaire insuffisant ({compte.Solde:N0} Ar).");

        var fournisseur = await _uow.Fournisseurs.GetByIdAsync(dette.FournisseurId, ct);

        await _uow.BeginTransactionAsync(ct);
        try
        {
            // 1) Mouvement bancaire (virement sortant)
            await _uow.MouvementsBancaires.AddAsync(new MouvementBancaire
            {
                CompteBancaireId = compteId,
                Type = TypeMouvementBancaire.Virement,
                Montant = montant,
                Beneficiaire = fournisseur?.Nom,
                Motif = $"Paiement dette : {dette.Libelle}",
                ChantierId = dette.ChantierId,
                FournisseurId = dette.FournisseurId,
                DetteFournisseurId = dette.Id,
                Date = DateTime.UtcNow
            }, ct);
            compte.Solde -= montant;
            _uow.ComptesBancaires.Update(compte);

            // 2) Mise à jour de la dette
            dette.MontantPaye += montant;
            dette.Statut = dette.SoldeRestant <= 0
                ? StatutDette.Soldee
                : StatutDette.PartiellementPayee;
            _uow.DettesFournisseurs.Update(dette);

            // 3) Dépense + impact Budget Comptes (optionnel)
            if (genererDepense)
            {
                var budget = (await _uow.BudgetsComptes.ListAsync(bg => bg.EstActif, ct))
                    .OrderByDescending(bg => bg.Annee).FirstOrDefault();
                if (budget is not null)
                {
                    budget.MontantConsomme += montant;
                    _uow.BudgetsComptes.Update(budget);
                }

                if (dette.ChantierId.HasValue)
                {
                    await _uow.Depenses.AddAsync(new Depense
                    {
                        Date = DateTime.UtcNow,
                        ChantierId = dette.ChantierId.Value,
                        Categorie = "Paiement dette",
                        Designation = $"{fournisseur?.Nom} - {dette.Libelle}",
                        Quantite = 1,
                        PrixUnitaire = montant,
                        BudgetConcerne = TypeBudget.Compte,
                        Justificatif = $"Virement {compte.Banque}",
                        Observation = "Généré automatiquement depuis le paiement de dette."
                    }, ct);
                }
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            await _audit.LogAsync(TypeActionAudit.Validation, nameof(DetteFournisseur), detteId.ToString(),
                nouvelleValeur: $"Paiement {montant:N0} Ar ({dette.Statut})", ct: ct);
            _logger.LogInformation("Dette {Id} payée {Montant} (statut {Statut}).", detteId, montant, dette.Statut);
            return Result.Success();
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync(ct);
            _logger.LogError(ex, "Échec du paiement de la dette {Id}.", detteId);
            return Result.Failure("Erreur lors du paiement : " + ex.Message);
        }
    }
}
