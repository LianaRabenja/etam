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
            $"À valider par la Finance — {p.Reference}",
            $"{p.Chantier?.Nom} — journée du {p.DatePrevision:dd/MM/yyyy}, {p.Total:N0} Ar demandés. "
            + "En attente de la validation du Responsable Financier.", p.ChantierId, ct);
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
            $"À valider par l'Administrateur — {p.Reference}",
            $"{p.Chantier?.Nom} — journée du {p.DatePrevision:dd/MM/yyyy}, {p.Total:N0} Ar. "
            + "La Finance a donné son accord. En attente de la validation de l'Administrateur.", p.ChantierId, ct);
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

        // Dernière étape, et la plus facile à oublier : une prévision validée ne
        // sert à rien tant qu'elle n'est pas exécutée — c'est l'exécution qui sort
        // l'argent de la banque. Sans cette alerte, le chantier attend sans savoir
        // pourquoi, et personne ne voit que le geste manque.
        await _alertes.CreerAsync(TypeAlerte.PrevisionEnAttente, NiveauAlerte.Avertissement,
            $"À exécuter — {p.Reference}",
            $"{p.Chantier?.Nom} — journée du {p.DatePrevision:dd/MM/yyyy}, {p.Total:N0} Ar validés. "
            + "Exécutez la prévision pour que l'argent sorte de la banque et parte au chantier.",
            p.ChantierId, ct);

        // Et celui qui a demandé l'argent doit l'apprendre : c'est la seule
        // notification qui concerne le Chef de chantier.
        await _alertes.CreerAsync(TypeAlerte.DecisionPrevision, NiveauAlerte.Info,
            $"Prévision validée — {p.Reference}",
            $"{p.Chantier?.Nom} — journée du {p.DatePrevision:dd/MM/yyyy}, {p.Total:N0} Ar. "
            + "La Direction a donné son accord définitif.", p.ChantierId, ct);

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

        await _alertes.CreerAsync(TypeAlerte.DecisionPrevision, NiveauAlerte.Avertissement,
            $"Prévision refusée — {p.Reference}",
            $"{p.Chantier?.Nom} — journée du {p.DatePrevision:dd/MM/yyyy}, {p.Total:N0} Ar. "
            + $"Motif : {motif}", p.ChantierId, ct);

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

            // --- Plus aucun report d'une journée sur l'autre ---
            // L'argent sort de la banque à l'exécution ET il est dépensé : il ne reste
            // rien dans les mains de personne, donc rien à reporter au lendemain.
            //
            // L'ancien calcul s'appuyait sur MontantDecaisse, qui ne bouge plus jamais :
            // chaque journée héritait du total de la précédente et le plafond s'empilait
            // à l'infini (jour 3 = jour1 + jour2 + jour3). Le cumul se fait désormais au
            // niveau de la SEMAINE, contre la prévision de la semaine, qui est l'argent réel.
            //
            // Le lien vers la journée précédente reste posé pour la lecture de
            // l'historique, mais il ne porte plus d'argent.
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

                if (!dejaRepris) p.PrevisionPrecedenteId = precedente.Id;
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
                    $"reportés, déjà dépensé {enveloppe.MontantConsomme:N0} Ar.)");
            }

            // --- L'ARGENT SORT ICI ---
            // C'est l'exécution de la prévision réelle qui fait sortir l'argent de
            // la banque. Le reçu signé plus tard n'est qu'une trace écrite : il ne
            // déclenche aucun mouvement.
            //
            // Seul le montant demandé ce jour sort. Le reliquat de la veille n'est
            // pas redébité : il est déjà entre les mains du chef depuis hier.
            var compte = await _uow.ComptesBancaires.Query()
                .FirstOrDefaultAsync(c => c.ChantierId == p.ChantierId && c.EstActif, ct);

            if (compte is null)
            {
                await _uow.RollbackAsync(ct);
                return Result.Failure(
                    $"Aucun compte bancaire actif pour {chantier.Nom}. " +
                    "Créez son compte dans le menu Banques avant d'exécuter une prévision.");
            }

            if (compte.Solde < total)
            {
                await _uow.RollbackAsync(ct);
                return Result.Failure(
                    $"Solde insuffisant sur {compte.Nom} : {compte.Solde:N0} Ar disponibles " +
                    $"pour une prévision de {total:N0} Ar.");
            }

            compte.Solde -= total;
            _uow.ComptesBancaires.Update(compte);

            await _uow.MouvementsBancaires.AddAsync(new MouvementBancaire
            {
                CompteBancaireId = compte.Id,
                Date = DateTime.UtcNow,
                Type = TypeMouvementBancaire.Retrait,
                Montant = total,
                Beneficiaire = chantier.Responsable ?? "Chef de chantier",
                Motif = $"{p.Reference} — prévision réelle du {p.DatePrevision:dd/MM/yyyy}",
                ChantierId = p.ChantierId,
                EstValide = true
            }, ct);

            // --- L'ENVELOPPE DU MOIS SE REMPLIT ICI ---
            // Personne ne l'incrémentait : MontantConsomme n'était écrit nulle part dans
            // le projet, il était seulement lu. Le contrôle juste au-dessus passait donc
            // toujours, et un chantier pouvait dépenser dix fois son enveloppe du mois
            // sans que rien ne s'y oppose. L'argent étant dépensé à l'exécution, c'est
            // ici que le mois se consomme.
            enveloppe.MontantConsomme += total;
            _uow.PrevisionsMensuelles.Update(enveloppe);

            p.PrevisionMensuelleId = enveloppe.Id;
            p.ReportVeille = report;
            p.MontantDecaisse = 0m;
            p.Statut = StatutPrevision.Executee;
            p.DateExecution = DateTime.UtcNow;
            _uow.Previsions.Update(p);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            await _alertes.EvaluerAlertesAsync(ct);

            // L'argent vient de sortir : on confronte tout de suite la semaine à son plan.
            await EvaluerPlanSemaineAsync(p, chantier.Nom, ct);

            await _audit.LogAsync(TypeActionAudit.Execution, nameof(PrevisionJournaliere), id.ToString(),
                nouvelleValeur: $"Enveloppe ouverte : {total:N0} Ar + report {report:N0} Ar", ct: ct);

            _logger.LogInformation(
                "Prévision réelle {Ref} exécutée sur {Chantier} : {Total} Ar sortis de la banque, " +
                "plafond du jour {Plafond} Ar (dont {Report} Ar reportés de la veille).",
                p.Reference, chantier.Nom, total, total + report, report);

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
    /// Le bénéficiaire signe le reçu sur le chantier.
    ///
    /// L'argent est déjà sorti de la banque à l'exécution de la prévision réelle.
    /// Cette signature ne déplace aucun argent : c'est la trace écrite attestant
    /// que la personne l'a bien reçu. Elle conditionne en revanche la saisie des
    /// sorties : on ne note pas de paiements sur un argent dont personne n'a
    /// reconnu la réception.
    /// </summary>
    public async Task<Result> AccuserReceptionAsync(long id, string nomSignataire, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nomSignataire))
            return Result.Failure("Le nom du signataire est obligatoire.");

        var p = await ChargerAsync(id, ct);
        if (p is null) return Result.Failure("Prévision introuvable.");
        if (p.Statut != StatutPrevision.Executee)
            return Result.Failure("Seule une prévision exécutée peut faire l'objet d'un reçu.");
        if (p.DateAccuseReception.HasValue)
            return Result.Failure("Le reçu de cette journée a déjà été signé.");

        p.AccuseReceptionParId = _currentUser.UserId;
        p.AccuseNomSignataire = nomSignataire.Trim();
        p.DateAccuseReception = DateTime.UtcNow;
        p.MontantAccuse = p.Lignes.Where(l => !l.IsDeleted).Sum(l => l.Total) + p.ReportVeille;
        _uow.Previsions.Update(p);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(TypeActionAudit.Validation, nameof(PrevisionJournaliere), id.ToString(),
            nouvelleValeur: $"Reçu signé par {nomSignataire.Trim()} — {p.MontantAccuse:N0} Ar", ct: ct);

        _logger.LogInformation("Reçu de {Ref} signé par {Nom}.", p.Reference, nomSignataire);
        return Result.Success();
    }

    /// <summary>
    /// Clôture de la semaine d'un chantier : on arrête les comptes de la semaine et
    /// on constate l'écart entre la prévision de la semaine et ce qui a été dépensé.
    ///
    /// AUCUN MOUVEMENT BANCAIRE N'EST FAIT ICI, et c'est volontaire. L'argent quitte
    /// la banque à l'exécution d'une prévision, et il est dépensé à ce moment-là. Ce
    /// qui n'a pas été exécuté n'a donc JAMAIS quitté le compte : il n'y a rien à y
    /// remettre. L'ancienne version créditait le compte du cumul de la période, ce qui
    /// revenait à créer de l'argent qui n'était jamais parti.
    ///
    /// Retourne la part de la prévision de la semaine qui n'a pas été dépensée. C'est une
    /// information — pas un transfert : cet argent est resté en banque.
    /// </summary>
    public async Task<Result<decimal>> CloturerPeriodeAsync(long chantierId, CancellationToken ct = default)
    {
        var chantier = await _uow.Chantiers.GetByIdAsync(chantierId, ct);
        if (chantier is null) return Result<decimal>.Failure("Chantier introuvable.");

        // La semaine à clôturer est celle de la dernière journée exécutée et non
        // encore clôturée. On ne clôture jamais « tout depuis le début » : une
        // semaine à la fois, dans l'ordre, sinon les constats se mélangent.
        var derniere = await _uow.Previsions.Query().AsNoTracking()
            .Where(p => p.ChantierId == chantierId
                        && p.DateRestitution == null
                        && (p.Statut == StatutPrevision.Executee
                            || p.Statut == StatutPrevision.RapportSoumis
                            || p.Statut == StatutPrevision.Cloturee))
            .OrderBy(p => p.DatePrevision).ThenBy(p => p.Id)
            .FirstOrDefaultAsync(ct);

        if (derniere is null)
            return Result<decimal>.Failure(
                $"Aucune semaine ouverte sur {chantier.Nom} : il n'y a rien à clôturer.");

        var jour = derniere.DatePrevision.Date;
        var decalage = ((int)jour.DayOfWeek + 6) % 7;
        var lundi = DateTime.SpecifyKind(jour.AddDays(-decalage), DateTimeKind.Utc);
        var dimanche = lundi.AddDays(6);

        var journees = await _uow.Previsions.Query()
            .Include(p => p.Lignes)
            .Where(p => p.ChantierId == chantierId
                        && p.DatePrevision >= lundi && p.DatePrevision <= dimanche
                        && p.DateRestitution == null
                        && (p.Statut == StatutPrevision.Executee
                            || p.Statut == StatutPrevision.RapportSoumis
                            || p.Statut == StatutPrevision.Cloturee))
            .ToListAsync(ct);

        // La prévision de la semaine : c'est elle qui fait référence, c'est l'argent réel.
        var prevuSemaine = await _uow.PlansJournaliers.Query().AsNoTracking()
            .Where(x => x.ChantierId == chantierId && x.Date >= lundi && x.Date <= dimanche)
            .SumAsync(x => (decimal?)x.MontantPrevu, ct) ?? 0m;

        var sortiSemaine = journees.Sum(p => p.Lignes.Where(l => !l.IsDeleted).Sum(l => l.Total));
        var nonDepense = prevuSemaine - sortiSemaine;

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var maintenant = DateTime.UtcNow;
            foreach (var j in journees)
            {
                // MontantRestitue reste à zéro : rien n'est physiquement rendu, rien
                // n'était resté dehors. Seule la date marque la semaine comme arrêtée.
                j.MontantRestitue = 0m;
                j.DateRestitution = maintenant;
                j.RestitueParId = _currentUser.UserId;
                _uow.Previsions.Update(j);
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            await _audit.LogAsync(TypeActionAudit.Validation, nameof(PrevisionJournaliere),
                chantierId.ToString(),
                nouvelleValeur: $"Semaine du {lundi:dd/MM/yyyy} clôturée — prévu {prevuSemaine:N0} Ar, "
                                + $"dépensé {sortiSemaine:N0} Ar, non dépensé {nonDepense:N0} Ar", ct: ct);

            _logger.LogInformation(
                "Semaine du {Lundi:dd/MM} clôturée sur {Chantier} : prévu {Prevu} Ar, "
                + "dépensé {Sorti} Ar, non dépensé {Reste} Ar ({Nb} journées).",
                lundi, chantier.Nom, prevuSemaine, sortiSemaine, nonDepense, journees.Count);

            return Result<decimal>.Success(nonDepense);
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync(ct);
            _logger.LogError(ex, "Échec de la clôture de semaine sur le chantier {Id}.", chantierId);
            return Result<decimal>.Failure("Une erreur est survenue lors de la clôture : " + ex.Message);
        }
    }

    /// <summary>
    /// Compare ce qui a été exécuté dans la semaine à ce que la prévision de la semaine
    /// prévoyait, et prévient quand on s'en approche ou qu'on le dépasse.
    ///
    /// C'est la référence demandée : l'argent étant utilisé dès l'exécution, la
    /// seule question qui reste est « a-t-on dépensé ce qui était écrit ? ». On la
    /// pose au moment où l'argent sort, pas des heures plus tard.
    /// </summary>
    private async Task EvaluerPlanSemaineAsync(PrevisionJournaliere p, string nomChantier, CancellationToken ct)
    {
        // Lundi de la semaine de cette journée. Le décalage de 6 place dimanche
        // en fin de semaine, comme à Madagascar.
        var jour = p.DatePrevision.Date;
        var decalage = ((int)jour.DayOfWeek + 6) % 7;
        var lundi = DateTime.SpecifyKind(jour.AddDays(-decalage), DateTimeKind.Utc);
        var dimanche = lundi.AddDays(6);

        var prevuSemaine = await _uow.PlansJournaliers.Query().AsNoTracking()
            .Where(x => x.ChantierId == p.ChantierId && x.Date >= lundi && x.Date <= dimanche)
            .SumAsync(x => (decimal?)x.MontantPrevu, ct) ?? 0m;

        // Pas de prévision pour cette semaine : rien à comparer, on ne dit rien.
        if (prevuSemaine <= 0) return;

        var journeesSorties = _uow.Previsions.Query()
            .Where(x => x.ChantierId == p.ChantierId
                        && x.DatePrevision >= lundi && x.DatePrevision <= dimanche
                        && (x.Statut == StatutPrevision.Executee
                            || x.Statut == StatutPrevision.RapportSoumis
                            || x.Statut == StatutPrevision.Cloturee));

        var sortiSemaine = await _uow.PrevisionLignes.Query().AsNoTracking()
            .Where(l => journeesSorties.Any(x => x.Id == l.PrevisionJournaliereId))
            .SumAsync(l => (decimal?)(l.Quantite * l.PrixUnitaireEstime), ct) ?? 0m;

        var pourcentage = (double)(sortiSemaine / prevuSemaine) * 100d;
        var periode = $"semaine du {lundi:dd/MM} au {dimanche:dd/MM}";

        if (sortiSemaine > prevuSemaine)
        {
            await _alertes.CreerAsync(TypeAlerte.DepassementPrevision, NiveauAlerte.Critique,
                $"Prévision de la semaine dépassée — {nomChantier}",
                $"{periode} : {sortiSemaine:N0} Ar dépensés pour une prévision de {prevuSemaine:N0} Ar. "
                + $"Dépassement de {sortiSemaine - prevuSemaine:N0} Ar.",
                p.ChantierId, ct);
            return;
        }

        // « Presque atteint » : on prévient à 80 % et à 90 %, pas à chaque sortie.
        var palier = pourcentage >= 90 ? 90 : pourcentage >= 80 ? 80 : 0;
        if (palier == 0) return;

        await _alertes.CreerAsync(TypeAlerte.SeuilMoitie, NiveauAlerte.Avertissement,
            $"Prévision de la semaine à {palier} % — {nomChantier}",
            $"{periode} : {sortiSemaine:N0} Ar dépensés sur {prevuSemaine:N0} Ar prévus. "
            + $"Il reste {prevuSemaine - sortiSemaine:N0} Ar avant d'atteindre la prévision.",
            p.ChantierId, ct);
    }

    private async Task<PrevisionJournaliere?> ChargerAsync(long id, CancellationToken ct)
    {
        return await _uow.Previsions.Query()
            .Include(p => p.Lignes)
            .Include(p => p.Chantier)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
    }
}
