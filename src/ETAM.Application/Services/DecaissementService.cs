using ETAM.Application.Common.Models;
using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ETAM.Application.Services;

/// <summary>
/// Décaissements : le seul endroit du logiciel où l'argent quitte réellement la banque.
///
/// Chaîne de contrôles, dans l'ordre :
///   1. la prévision est ouverte et le chef a signé la réception de l'enveloppe
///   2. le montant tient dans le reliquat de la journée (demandé + report de la veille)
///   3. le montant tient dans l'enveloppe du mois
///   4. le budget concerné (Matériel du chantier ou Comptes de l'entreprise) le supporte
///   5. le compte bancaire est suffisamment approvisionné
/// </summary>
public class DecaissementService : IDecaissementService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;
    private readonly IAlerteService _alertes;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DecaissementService> _logger;

    public DecaissementService(
        IUnitOfWork uow,
        IAuditService audit,
        IAlerteService alertes,
        ICurrentUserService currentUser,
        ILogger<DecaissementService> logger)
    {
        _uow = uow;
        _audit = audit;
        _alertes = alertes;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<long>> EnregistrerAsync(DecaissementDto dto, CancellationToken ct = default)
    {
        if (dto.Montant <= 0) return Result<long>.Failure("Le montant doit être supérieur à zéro.");
        if (string.IsNullOrWhiteSpace(dto.Beneficiaire))
            return Result<long>.Failure("Le bénéficiaire est obligatoire : il faut savoir qui a reçu l'argent.");
        if (string.IsNullOrWhiteSpace(dto.Motif))
            return Result<long>.Failure("Le motif est obligatoire.");

        var prevision = await _uow.Previsions.Query()
            .Include(p => p.Lignes)
            .FirstOrDefaultAsync(p => p.Id == dto.PrevisionJournaliereId, ct);
        if (prevision is null) return Result<long>.Failure("Prévision introuvable.");

        // --- 1. La prévision doit être ouverte et l'argent reconnu reçu ---
        if (prevision.Statut != StatutPrevision.Executee)
            return Result<long>.Failure(
                "Cette prévision n'est pas ouverte. Seule une prévision exécutée permet de décaisser.");

        if (!prevision.DateAccuseReception.HasValue)
            return Result<long>.Failure(
                "Le chef de chantier n'a pas encore accusé réception de l'argent. " +
                "Aucun décaissement n'est possible tant que la réception n'est pas signée.");

        // --- 2. Le reliquat de la journée ---
        var reliquat = prevision.PlafondDuJour - prevision.MontantDecaisse;
        if (dto.Montant > reliquat)
            return Result<long>.Failure(
                $"Montant supérieur au reliquat de la journée. " +
                $"Plafond du jour {prevision.PlafondDuJour:N0} Ar " +
                $"(dont {prevision.ReportVeille:N0} Ar reportés de la veille), " +
                $"déjà décaissé {prevision.MontantDecaisse:N0} Ar, " +
                $"il reste {reliquat:N0} Ar.");

        var chantier = await _uow.Chantiers.GetByIdAsync(prevision.ChantierId, ct);
        if (chantier is null) return Result<long>.Failure("Chantier introuvable.");

        var compte = await _uow.ComptesBancaires.GetByIdAsync(dto.CompteBancaireId, ct);
        if (compte is null) return Result<long>.Failure("Compte bancaire introuvable.");
        if (!compte.EstActif) return Result<long>.Failure("Ce compte bancaire est clôturé.");

        // --- 5. Provision du compte ---
        if (compte.Solde < dto.Montant)
            return Result<long>.Failure(
                $"Solde insuffisant sur {compte.Nom} : {compte.Solde:N0} Ar disponibles " +
                $"pour un décaissement de {dto.Montant:N0} Ar.");

        // --- 3. L'enveloppe du mois ---
        PrevisionMensuelle? enveloppe = null;
        if (prevision.PrevisionMensuelleId.HasValue)
        {
            enveloppe = await _uow.PrevisionsMensuelles.GetByIdAsync(prevision.PrevisionMensuelleId.Value, ct);
            if (enveloppe is not null)
            {
                var dispoMois = enveloppe.EnveloppeTotale - enveloppe.MontantConsomme;
                if (dto.Montant > dispoMois)
                    return Result<long>.Failure(
                        $"Enveloppe de {enveloppe.Libelle} épuisée : il reste {dispoMois:N0} Ar " +
                        $"sur {enveloppe.EnveloppeTotale:N0} Ar " +
                        $"(dont {enveloppe.ReportMoisPrecedent:N0} Ar reportés du mois précédent).");
            }
        }

        // --- 4. Le budget concerné ---
        BudgetCompte? budget = null;
        if (dto.BudgetConcerne == TypeBudget.Materiel)
        {
            if (chantier.MaterielDisponible < dto.Montant)
                return Result<long>.Failure(
                    $"Budget Matériel insuffisant sur {chantier.Nom} : " +
                    $"{chantier.MaterielDisponible:N0} Ar disponibles " +
                    $"(transféré {chantier.MaterielTransfere:N0} Ar, consommé {chantier.Consommation:N0} Ar). " +
                    "Effectuez un transfert depuis la banque avant de décaisser.");
        }
        else
        {
            budget = (await _uow.BudgetsComptes.ListAsync(b => b.EstActif, ct))
                .OrderByDescending(b => b.Annee).FirstOrDefault();
            if (budget is null)
                return Result<long>.Failure("Aucun Budget Comptes actif : impossible d'imputer cette dépense.");
            if (budget.DisponibleReel < dto.Montant)
                return Result<long>.Failure(
                    $"Budget Comptes insuffisant : {budget.DisponibleReel:N0} Ar disponibles.");
        }

        var ligne = dto.PrevisionLigneId.HasValue
            ? prevision.Lignes.FirstOrDefault(l => l.Id == dto.PrevisionLigneId.Value && !l.IsDeleted)
            : null;

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var decaissement = new Decaissement
            {
                PrevisionJournaliereId = prevision.Id,
                PrevisionLigneId = ligne?.Id,
                Date = dto.Date,
                Beneficiaire = dto.Beneficiaire.Trim(),
                Motif = dto.Motif.Trim(),
                Montant = dto.Montant,
                Mode = dto.Mode,
                CompteBancaireId = compte.Id,
                BudgetConcerne = dto.BudgetConcerne,
                Reference = dto.Reference,
                Observation = dto.Observation,
                AccuseNom = dto.AccuseNom,
                DateAccuse = string.IsNullOrWhiteSpace(dto.AccuseNom) ? null : DateTime.UtcNow
            };
            await _uow.Decaissements.AddAsync(decaissement, ct);

            // --- L'argent sort de la banque ---
            compte.Solde -= dto.Montant;
            _uow.ComptesBancaires.Update(compte);

            await _uow.MouvementsBancaires.AddAsync(new MouvementBancaire
            {
                CompteBancaireId = compte.Id,
                Date = dto.Date,
                Type = dto.Mode == ModePaiement.Especes
                    ? TypeMouvementBancaire.Retrait
                    : TypeMouvementBancaire.Virement,
                Montant = dto.Montant,
                Beneficiaire = dto.Beneficiaire.Trim(),
                Motif = $"{prevision.Reference} — {dto.Motif.Trim()}",
                Reference = dto.Reference,
                ChantierId = prevision.ChantierId,
                EstValide = true
            }, ct);

            // --- Les compteurs de consommation ---
            if (dto.BudgetConcerne == TypeBudget.Materiel)
            {
                chantier.Consommation += dto.Montant;
                _uow.Chantiers.Update(chantier);
            }
            else if (budget is not null)
            {
                budget.MontantConsomme += dto.Montant;
                _uow.BudgetsComptes.Update(budget);
            }

            prevision.MontantDecaisse += dto.Montant;
            _uow.Previsions.Update(prevision);

            if (enveloppe is not null)
            {
                enveloppe.MontantConsomme += dto.Montant;
                _uow.PrevisionsMensuelles.Update(enveloppe);
            }

            // --- Stock : sortie proportionnelle à la part payée de la ligne ---
            if (ligne is not null && ligne.MateriauId.HasValue && ligne.Total > 0)
            {
                var materiau = await _uow.Materiaux.GetByIdAsync(ligne.MateriauId.Value, ct);
                if (materiau is not null)
                {
                    var part = dto.Montant / ligne.Total;
                    if (part > 1m) part = 1m;
                    var quantiteSortie = Math.Round(ligne.Quantite * part, 3);

                    materiau.QuantiteUtilisee += quantiteSortie;
                    _uow.Materiaux.Update(materiau);

                    await _uow.MouvementsMateriau.AddAsync(new MouvementMateriau
                    {
                        MateriauxId = materiau.Id,
                        DateMouvement = dto.Date,
                        QuantiteSortie = quantiteSortie,
                        SoldeSurBesoin = materiau.Besoin - materiau.QuantiteUtilisee,
                        SoldeEnStock = materiau.StockDisponible,
                        Motif = $"{prevision.Reference} — {dto.Motif.Trim()}"
                    }, ct);
                }
            }

            // --- Dette : le paiement la fait diminuer ---
            if (ligne is not null && ligne.DetteFournisseurId.HasValue)
            {
                var dette = await _uow.DettesFournisseurs.GetByIdAsync(ligne.DetteFournisseurId.Value, ct);
                if (dette is not null)
                {
                    dette.MontantPaye += dto.Montant;
                    dette.Statut = dette.SoldeRestant <= 0
                        ? StatutDette.Soldee
                        : StatutDette.PartiellementPayee;
                    _uow.DettesFournisseurs.Update(dette);
                }
            }

            // --- Journal des dépenses ---
            await _uow.Depenses.AddAsync(new Depense
            {
                Date = dto.Date,
                ChantierId = prevision.ChantierId,
                PrevisionJournaliereId = prevision.Id,
                Categorie = ligne?.Categorie ?? "Décaissement",
                Designation = dto.Motif.Trim(),
                Quantite = 1,
                PrixUnitaire = dto.Montant,
                BudgetConcerne = dto.BudgetConcerne,
                Justificatif = dto.Reference,
                Observation = $"Bénéficiaire : {dto.Beneficiaire.Trim()}"
            }, ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            await _alertes.EvaluerAlertesAsync(ct);
            await _audit.LogAsync(TypeActionAudit.Execution, nameof(Decaissement), decaissement.Id.ToString(),
                nouvelleValeur: $"{dto.Montant:N0} Ar à {dto.Beneficiaire} — {dto.Motif}", ct: ct);

            _logger.LogInformation("Décaissement de {Montant} Ar sur {Ref} (reste {Reste} Ar).",
                dto.Montant, prevision.Reference, reliquat - dto.Montant);

            return Result<long>.Success(decaissement.Id);
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync(ct);
            _logger.LogError(ex, "Échec du décaissement sur la prévision {Id}.", dto.PrevisionJournaliereId);
            return Result<long>.Failure("Erreur lors de l'enregistrement : " + ex.Message);
        }
    }

    public async Task<Result> AnnulerAsync(long id, string motif, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(motif))
            return Result.Failure("Un motif d'annulation est obligatoire.");

        var d = await _uow.Decaissements.Query()
            .Include(x => x.PrevisionJournaliere)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (d is null) return Result.Failure("Décaissement introuvable.");

        var prevision = d.PrevisionJournaliere;
        if (prevision.Statut == StatutPrevision.Cloturee)
            return Result.Failure(
                "Les travaux de cette prévision ont été réceptionnés : le décaissement ne peut plus être annulé.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            // Remise en état, dans l'ordre inverse de l'enregistrement.
            var compte = await _uow.ComptesBancaires.GetByIdAsync(d.CompteBancaireId, ct);
            if (compte is not null)
            {
                compte.Solde += d.Montant;
                _uow.ComptesBancaires.Update(compte);
            }

            if (d.BudgetConcerne == TypeBudget.Materiel)
            {
                var chantier = await _uow.Chantiers.GetByIdAsync(prevision.ChantierId, ct);
                if (chantier is not null)
                {
                    chantier.Consommation -= d.Montant;
                    if (chantier.Consommation < 0) chantier.Consommation = 0;
                    _uow.Chantiers.Update(chantier);
                }
            }
            else
            {
                var budget = (await _uow.BudgetsComptes.ListAsync(b => b.EstActif, ct))
                    .OrderByDescending(b => b.Annee).FirstOrDefault();
                if (budget is not null)
                {
                    budget.MontantConsomme -= d.Montant;
                    if (budget.MontantConsomme < 0) budget.MontantConsomme = 0;
                    _uow.BudgetsComptes.Update(budget);
                }
            }

            prevision.MontantDecaisse -= d.Montant;
            if (prevision.MontantDecaisse < 0) prevision.MontantDecaisse = 0;
            _uow.Previsions.Update(prevision);

            if (prevision.PrevisionMensuelleId.HasValue)
            {
                var enveloppe = await _uow.PrevisionsMensuelles.GetByIdAsync(prevision.PrevisionMensuelleId.Value, ct);
                if (enveloppe is not null)
                {
                    enveloppe.MontantConsomme -= d.Montant;
                    if (enveloppe.MontantConsomme < 0) enveloppe.MontantConsomme = 0;
                    _uow.PrevisionsMensuelles.Update(enveloppe);
                }
            }

            // Mouvement bancaire de contre-passation : on ne supprime jamais une écriture.
            await _uow.MouvementsBancaires.AddAsync(new MouvementBancaire
            {
                CompteBancaireId = d.CompteBancaireId,
                Date = DateTime.UtcNow,
                Type = TypeMouvementBancaire.Depot,
                Montant = d.Montant,
                Beneficiaire = d.Beneficiaire,
                Motif = $"Annulation décaissement — {motif}",
                Reference = d.Reference,
                ChantierId = prevision.ChantierId,
                EstValide = true
            }, ct);

            d.Observation = $"{d.Observation} | ANNULÉ le {DateTime.UtcNow:dd/MM/yyyy} par " +
                            $"{_currentUser.UserName} : {motif}";
            d.IsDeleted = true;
            _uow.Decaissements.Update(d);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync(ct);
            _logger.LogError(ex, "Échec de l'annulation du décaissement {Id}.", id);
            return Result.Failure("Erreur lors de l'annulation : " + ex.Message);
        }

        await _audit.LogAsync(TypeActionAudit.Suppression, nameof(Decaissement), id.ToString(),
            nouvelleValeur: motif, ct: ct);
        return Result.Success();
    }
}
