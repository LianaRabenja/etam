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
    /// Ouvre l'enveloppe de la journée.
    ///
    /// ATTENTION : cette opération ne sort PLUS d'argent de la banque et n'impute plus
    /// les budgets. Elle se contente d'autoriser un plafond de dépense pour la journée,
    /// en y ajoutant le reliquat non décaissé de la prévision précédente du même chantier.
    /// L'argent ne bouge qu'au moment des décaissements (voir DecaissementService).
    /// </summary>
    public async Task<Result> ExecuterAsync(long id, bool utiliserReserve = false, CancellationToken ct = default)
    {
        var p = await ChargerAsync(id, ct);
        if (p is null) return Result.Failure("Prévision introuvable.");
        if (p.Statut != StatutPrevision.ValideeAdministrateur)
            return Result.Failure("Seule une prévision validée par l'Administrateur peut être ouverte.");

        var chantier = await _uow.Chantiers.GetByIdAsync(p.ChantierId, ct);
        if (chantier is null) return Result.Failure("Chantier introuvable.");

        var total = p.Lignes.Where(l => !l.IsDeleted).Sum(l => l.Total);
        if (total <= 0) return Result.Failure("Cette prévision ne comporte aucune ligne chiffrée.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            // --- Rattachement à l'enveloppe du mois ---
            var enveloppe = await _uow.PrevisionsMensuelles.Query()
                .FirstOrDefaultAsync(m => m.ChantierId == p.ChantierId
                                          && m.Annee == p.DatePrevision.Year
                                          && m.Mois == p.DatePrevision.Month
                                          && m.Statut == StatutPrevisionMensuelle.Validee, ct);

            if (enveloppe is null)
            {
                await _uow.RollbackAsync(ct);
                return Result.Failure(
                    $"Aucune enveloppe mensuelle ouverte pour {PrevisionMensuelle.NomDuMois(p.DatePrevision.Month)} " +
                    $"{p.DatePrevision.Year} sur {chantier.Nom}. " +
                    "Créez et validez l'enveloppe du mois avant d'ouvrir une prévision journalière.");
            }

            // --- Reprise du reliquat de la journée précédente ---
            // On prend la dernière prévision ouverte ou clôturée du chantier, antérieure
            // à celle-ci, et dont le reliquat n'a pas déjà été repris par une autre.
            var precedente = await _uow.Previsions.Query()
                .Where(x => x.ChantierId == p.ChantierId
                            && x.Id != p.Id
                            && x.DatePrevision < p.DatePrevision
                            && (x.Statut == StatutPrevision.Executee
                                || x.Statut == StatutPrevision.RapportSoumis
                                || x.Statut == StatutPrevision.Cloturee))
                .OrderByDescending(x => x.DatePrevision).ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync(ct);

            var report = 0m;
            if (precedente is not null)
            {
                var dejaRepris = await _uow.Previsions.AnyAsync(
                    x => x.PrevisionPrecedenteId == precedente.Id && x.Id != p.Id, ct);

                if (!dejaRepris)
                {
                    // Le reliquat se recalcule à partir des montants figés sur la prévision.
                    // Les parenthèses sont indispensables : sans elles, une prévision sans
                    // ligne ferait retomber le plafond à zéro en perdant le report.
                    var totalPrecedent = await _uow.PrevisionLignes.Query()
                        .Where(l => l.PrevisionJournaliereId == precedente.Id)
                        .SumAsync(l => (decimal?)(l.Quantite * l.PrixUnitaireEstime), ct) ?? 0m;

                    var plafondPrecedent = precedente.ReportVeille + totalPrecedent;
                    report = plafondPrecedent - precedente.MontantDecaisse;
                    if (report < 0) report = 0;
                    p.PrevisionPrecedenteId = precedente.Id;
                }
            }

            // --- L'enveloppe du mois doit pouvoir couvrir la journée ---
            var disponibleMois = enveloppe.EnveloppeTotale - enveloppe.MontantConsomme;
            if (total > disponibleMois)
            {
                await _uow.RollbackAsync(ct);
                return Result.Failure(
                    $"Enveloppe de {enveloppe.Libelle} insuffisante : {disponibleMois:N0} Ar disponibles " +
                    $"pour une prévision de {total:N0} Ar. " +
                    $"(Enveloppe {enveloppe.EnveloppeTotale:N0} Ar dont {enveloppe.ReportMoisPrecedent:N0} Ar " +
                    $"reportés, déjà décaissé {enveloppe.MontantConsomme:N0} Ar.)");
            }

            // L'argent ne sort PAS ici. L'exécution ouvre l'enveloppe et réserve le
            // montant sur le mois ; la banque n'est débitée qu'au moment où le
            // bénéficiaire signe la réception sur le chantier (AccuserReceptionAsync).
            // On vérifie tout de même dès maintenant que le compte pourra suivre,
            // pour ne pas ouvrir une enveloppe qu'on ne saura pas honorer.
            var compte = await _uow.ComptesBancaires.Query().AsNoTracking()
                .FirstOrDefaultAsync(c => c.ChantierId == p.ChantierId && c.EstActif, ct);

            if (compte is null)
            {
                await _uow.RollbackAsync(ct);
                return Result.Failure(
                    $"Aucun compte bancaire actif pour {chantier.Nom}. " +
                    "Créez son compte dans le menu Banques avant d'ouvrir une prévision.");
            }

            if (compte.Solde < total)
            {
                await _uow.RollbackAsync(ct);
                return Result.Failure(
                    $"Solde insuffisant sur {compte.Nom} : {compte.Solde:N0} Ar disponibles " +
                    $"pour une prévision de {total:N0} Ar.");
            }

            p.PrevisionMensuelleId = enveloppe.Id;
            p.ReportVeille = report;
            p.MontantDecaisse = 0m;
            p.Statut = StatutPrevision.Executee;
            p.DateExecution = DateTime.UtcNow;
            _uow.Previsions.Update(p);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            await _alertes.EvaluerAlertesAsync(ct);
            await _audit.LogAsync(TypeActionAudit.Execution, nameof(PrevisionJournaliere), id.ToString(),
                nouvelleValeur: $"Enveloppe ouverte : {total:N0} Ar + report {report:N0} Ar", ct: ct);

            _logger.LogInformation(
                "Prévision {Ref} ouverte sur {Chantier} : plafond du jour {Plafond} Ar " +
                "(dont {Report} Ar reportés). L'argent sortira à la signature du reçu.",
                p.Reference, chantier.Nom, total + report, report);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync(ct);
            _logger.LogError(ex, "Échec de l'ouverture de la prévision {Id}.", id);
            return Result.Failure("Une erreur est survenue lors de l'ouverture : " + ex.Message);
        }
    }

    /// <summary>
    /// Le bénéficiaire signe la réception de l'argent, sur le chantier.
    ///
    /// C'EST ICI que l'argent quitte réellement la banque : tant que personne n'a
    /// signé, le compte n'est pas touché. La signature et le débit sont donc
    /// indissociables — on ne peut pas avoir l'un sans l'autre.
    ///
    /// Seul le montant demandé pour la journée sort. Le reliquat de la veille n'est
    /// pas redébité : cet argent est déjà entre les mains du chef depuis hier.
    /// </summary>
    public async Task<Result> AccuserReceptionAsync(long id, string nomSignataire, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nomSignataire))
            return Result.Failure("Le nom du signataire est obligatoire.");

        var p = await ChargerAsync(id, ct);
        if (p is null) return Result.Failure("Prévision introuvable.");
        if (p.Statut != StatutPrevision.Executee)
            return Result.Failure("Seule une prévision ouverte peut faire l'objet d'un accusé de réception.");
        if (p.DateAccuseReception.HasValue)
            return Result.Failure("La réception de cette enveloppe a déjà été signée.");

        var chantier = await _uow.Chantiers.GetByIdAsync(p.ChantierId, ct);
        if (chantier is null) return Result.Failure("Chantier introuvable.");

        var montantASortir = p.Lignes.Where(l => !l.IsDeleted).Sum(l => l.Total);

        var compte = await _uow.ComptesBancaires.Query()
            .FirstOrDefaultAsync(c => c.ChantierId == p.ChantierId && c.EstActif, ct);
        if (compte is null)
            return Result.Failure($"Aucun compte bancaire actif pour {chantier.Nom}.");

        if (compte.Solde < montantASortir)
            return Result.Failure(
                $"Solde insuffisant sur {compte.Nom} : {compte.Solde:N0} Ar disponibles " +
                $"pour une remise de {montantASortir:N0} Ar.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            // L'argent sort du compte au moment de la signature.
            compte.Solde -= montantASortir;
            _uow.ComptesBancaires.Update(compte);

            await _uow.MouvementsBancaires.AddAsync(new MouvementBancaire
            {
                CompteBancaireId = compte.Id,
                Date = DateTime.UtcNow,
                Type = TypeMouvementBancaire.Retrait,
                Montant = montantASortir,
                Beneficiaire = nomSignataire.Trim(),
                Motif = $"{p.Reference} — reçu signé sur le chantier le {DateTime.UtcNow:dd/MM/yyyy}",
                ChantierId = p.ChantierId,
                EstValide = true
            }, ct);

            p.AccuseReceptionParId = _currentUser.UserId;
            p.AccuseNomSignataire = nomSignataire.Trim();
            p.DateAccuseReception = DateTime.UtcNow;
            p.MontantAccuse = montantASortir + p.ReportVeille;
            _uow.Previsions.Update(p);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync(ct);
            _logger.LogError(ex, "Échec de l'accusé de réception de la prévision {Id}.", id);
            return Result.Failure("Erreur lors de l'enregistrement du reçu : " + ex.Message);
        }

        await _audit.LogAsync(TypeActionAudit.Validation, nameof(PrevisionJournaliere), id.ToString(),
            nouvelleValeur: $"Reçu signé par {nomSignataire.Trim()} — {montantASortir:N0} Ar sortis de la banque",
            ct: ct);

        _logger.LogInformation(
            "Reçu de {Ref} signé par {Nom} : {Montant} Ar retirés du compte.",
            p.Reference, nomSignataire, montantASortir);
        return Result.Success();
    }

    private async Task<PrevisionJournaliere?> ChargerAsync(long id, CancellationToken ct)
    {
        return await _uow.Previsions.Query()
            .Include(p => p.Lignes)
            .Include(p => p.Chantier)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
    }
}
