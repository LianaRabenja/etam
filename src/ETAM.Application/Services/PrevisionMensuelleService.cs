using ETAM.Application.Common.Models;
using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ETAM.Application.Services;

/// <summary>
/// Gestion des enveloppes mensuelles.
///
/// Règle centrale : l'argent ne bouge jamais ici. Une enveloppe mensuelle est un
/// plafond d'autorisation. Seuls les décaissements débitent la banque. Le reliquat
/// d'un mois se reporte donc sur le suivant sans aucun mouvement d'argent.
/// </summary>
public class PrevisionMensuelleService : IPrevisionMensuelleService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<PrevisionMensuelleService> _logger;

    public PrevisionMensuelleService(
        IUnitOfWork uow,
        IAuditService audit,
        ICurrentUserService currentUser,
        ILogger<PrevisionMensuelleService> logger)
    {
        _uow = uow;
        _audit = audit;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<long>> CreerAsync(
        long chantierId, int annee, int mois, decimal montantPrevu,
        IEnumerable<(string Rubrique, string? Designation, decimal Montant, long? PrevisionGlobaleLigneId)>? lignes = null,
        string? observation = null, CancellationToken ct = default)
    {
        if (mois is < 1 or > 12) return Result<long>.Failure("Le mois doit être compris entre 1 et 12.");
        if (montantPrevu <= 0) return Result<long>.Failure("Le montant du mois doit être supérieur à zéro.");

        var chantier = await _uow.Chantiers.GetByIdAsync(chantierId, ct);
        if (chantier is null) return Result<long>.Failure("Chantier introuvable.");

        // --- Une seule enveloppe par chantier et par mois ---
        //
        // La base impose un index unique sur (ChantierId, Annee, Mois), et cet index
        // voit TOUTES les lignes : y compris celles qui sont refusées et celles qui
        // sont supprimées en douceur (IsDeleted). L'application, elle, ne voyait ni
        // les unes ni les autres — le filtre global masque les supprimées, et le test
        // écartait les refusées.
        //
        // Résultat : un mois refusé ou supprimé bloquait définitivement sa recréation,
        // avec une erreur 500 illisible au lieu d'un message. C'est le défaut corrigé ici.
        //
        // IgnoreQueryFilters() est indispensable : sans lui on ne verrait pas les lignes
        // supprimées, donc pas celles qui provoquent justement le conflit.
        var existante = await _uow.PrevisionsMensuelles.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.ChantierId == chantierId
                                   && m.Annee == annee
                                   && m.Mois == mois, ct);

        if (existante is not null)
        {
            var reutilisable = existante.IsDeleted
                            || existante.Statut == StatutPrevisionMensuelle.Refusee;

            if (!reutilisable)
                return Result<long>.Failure(
                    $"Une enveloppe existe déjà pour {PrevisionMensuelle.NomDuMois(mois)} {annee} " +
                    $"sur ce chantier ({existante.Reference}, {existante.Statut}). " +
                    "Ouvrez-la ou corrigez son montant plutôt que d'en créer une seconde.");

            // Le mois avait été refusé ou supprimé : on ressuscite la ligne existante
            // au lieu d'en insérer une seconde, qui violerait l'index unique.
            return await ReactiverAsync(existante, chantier, montantPrevu, lignes, observation, ct);
        }

        // Le total des mois ne peut pas dépasser le budget du projet.
        // Le report n'entre pas dans ce calcul : il a déjà été alloué le mois précédent.
        var dejaEngage = await TotalMoisEngagesAsync(chantierId, null, ct);
        if (dejaEngage + montantPrevu > chantier.BudgetProjet)
        {
            var reste = chantier.BudgetProjet - dejaEngage;
            return Result<long>.Failure(
                $"Budget projet dépassé : {dejaEngage:N0} Ar déjà répartis sur {chantier.BudgetProjet:N0} Ar. " +
                $"Il reste {reste:N0} Ar à répartir.");
        }

        // Reprise du reliquat du dernier mois clôturé de ce chantier.
        var moisPrecedent = await _uow.PrevisionsMensuelles.Query().AsNoTracking()
            .Where(m => m.ChantierId == chantierId
                        && m.Statut == StatutPrevisionMensuelle.Cloturee
                        && (m.Annee < annee || (m.Annee == annee && m.Mois < mois)))
            .OrderByDescending(m => m.Annee).ThenByDescending(m => m.Mois)
            .FirstOrDefaultAsync(ct);

        var report = 0m;
        long? precedentId = null;
        if (moisPrecedent is not null)
        {
            report = moisPrecedent.EnveloppeTotale - moisPrecedent.MontantConsomme;
            if (report < 0) report = 0;   // un dépassement ne se reporte pas en négatif
            precedentId = moisPrecedent.Id;
        }

        var planProjet = await _uow.PrevisionsGlobales.Query().AsNoTracking()
            .Where(g => g.ChantierId == chantierId
                        && (g.Statut == StatutPrevisionGlobale.MiseEnBanque
                            || g.Statut == StatutPrevisionGlobale.ValideeAdministrateur))
            .OrderByDescending(g => g.DateCreation)
            .FirstOrDefaultAsync(ct);

        var enveloppe = new PrevisionMensuelle
        {
            ChantierId = chantierId,
            PrevisionGlobaleId = planProjet?.Id,
            Annee = annee,
            Mois = mois,
            Reference = $"PMENS-{chantier.Code}-{annee:D4}{mois:D2}",
            MontantPrevu = montantPrevu,
            ReportMoisPrecedent = report,
            PrevisionMensuellePrecedenteId = precedentId,
            MontantConsomme = 0m,
            Statut = StatutPrevisionMensuelle.Brouillon,
            SoumisePar = _currentUser.UserName,
            DateSoumission = DateTime.UtcNow,
            Observation = observation
        };

        if (lignes is not null)
        {
            foreach (var l in lignes.Where(l => l.Montant > 0))
            {
                enveloppe.Lignes.Add(new PrevisionMensuelleLigne
                {
                    Rubrique = l.Rubrique,
                    Designation = l.Designation,
                    Montant = l.Montant,
                    PrevisionGlobaleLigneId = l.PrevisionGlobaleLigneId
                });
            }
        }

        await _uow.PrevisionsMensuelles.AddAsync(enveloppe, ct);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(TypeActionAudit.Ajout, nameof(PrevisionMensuelle), enveloppe.Id.ToString(),
            nouvelleValeur: $"{enveloppe.Reference} — {montantPrevu:N0} Ar + report {report:N0} Ar", ct: ct);

        _logger.LogInformation("Enveloppe {Ref} créée ({Montant} Ar, report {Report} Ar).",
            enveloppe.Reference, montantPrevu, report);

        return Result<long>.Success(enveloppe.Id);
    }

    /// <summary>
    /// Remet en service une enveloppe qui avait été refusée ou supprimée, au lieu d'en
    /// insérer une seconde pour le même mois — ce que l'index unique de la base refuse.
    /// L'utilisateur, lui, voit simplement son mois créé.
    /// </summary>
    private async Task<Result<long>> ReactiverAsync(
        PrevisionMensuelle m, Chantier chantier, decimal montantPrevu,
        IEnumerable<(string Rubrique, string? Designation, decimal Montant, long? PrevisionGlobaleLigneId)>? lignes,
        string? observation, CancellationToken ct)
    {
        // Le plafond du budget projet s'applique aussi à une réactivation.
        var dejaEngage = await TotalMoisEngagesAsync(chantier.Id, m.Id, ct);
        if (dejaEngage + montantPrevu > chantier.BudgetProjet)
            return Result<long>.Failure(
                $"Budget projet dépassé : {dejaEngage:N0} Ar déjà répartis sur {chantier.BudgetProjet:N0} Ar. " +
                $"Il reste {(chantier.BudgetProjet - dejaEngage):N0} Ar à répartir.");

        // Reprise du reliquat du dernier mois clôturé, comme à la création.
        var moisPrecedent = await _uow.PrevisionsMensuelles.Query().AsNoTracking()
            .Where(x => x.ChantierId == chantier.Id
                        && x.Id != m.Id
                        && x.Statut == StatutPrevisionMensuelle.Cloturee
                        && (x.Annee < m.Annee || (x.Annee == m.Annee && x.Mois < m.Mois)))
            .OrderByDescending(x => x.Annee).ThenByDescending(x => x.Mois)
            .FirstOrDefaultAsync(ct);

        var report = 0m;
        if (moisPrecedent is not null)
        {
            report = moisPrecedent.EnveloppeTotale - moisPrecedent.MontantConsomme;
            if (report < 0) report = 0;
        }

        m.IsDeleted        = false;
        m.Statut           = StatutPrevisionMensuelle.Brouillon;
        m.MotifRefus       = null;
        m.MontantPrevu     = montantPrevu;
        m.MontantConsomme  = 0m;
        m.ReportMoisPrecedent = report;
        m.PrevisionMensuellePrecedenteId = moisPrecedent?.Id;
        m.Observation      = observation;
        m.SoumisePar       = _currentUser.UserName;
        m.DateSoumission   = DateTime.UtcNow;
        m.DateValidation   = null;
        m.ValideeParId     = null;
        m.DateCloture      = null;
        m.ClotureeParId    = null;
        m.Reference        = $"PMENS-{chantier.Code}-{m.Annee:D4}{m.Mois:D2}";

        // Les anciennes lignes de répartition n'ont plus lieu d'être.
        foreach (var ancienne in await _uow.PrevisionMensuelleLignes.Query()
                     .IgnoreQueryFilters()
                     .Where(l => l.PrevisionMensuelleId == m.Id).ToListAsync(ct))
            _uow.PrevisionMensuelleLignes.Remove(ancienne);

        if (lignes is not null)
        {
            foreach (var l in lignes.Where(l => l.Montant > 0))
                await _uow.PrevisionMensuelleLignes.AddAsync(new PrevisionMensuelleLigne
                {
                    PrevisionMensuelleId = m.Id,
                    Rubrique = l.Rubrique,
                    Designation = l.Designation,
                    Montant = l.Montant,
                    PrevisionGlobaleLigneId = l.PrevisionGlobaleLigneId
                }, ct);
        }

        _uow.PrevisionsMensuelles.Update(m);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(TypeActionAudit.Ajout, nameof(PrevisionMensuelle), m.Id.ToString(),
            nouvelleValeur: $"{m.Reference} — reactivee, {montantPrevu:N0} Ar + report {report:N0} Ar", ct: ct);

        _logger.LogInformation("Enveloppe {Ref} reactivee ({Montant} Ar, report {Report} Ar).",
            m.Reference, montantPrevu, report);

        return Result<long>.Success(m.Id);
    }

    public async Task<Result> ValiderAsync(long id, CancellationToken ct = default)
    {
        var m = await _uow.PrevisionsMensuelles.GetByIdAsync(id, ct);
        if (m is null) return Result.Failure("Enveloppe mensuelle introuvable.");
        if (m.Statut != StatutPrevisionMensuelle.Brouillon)
            return Result.Failure("Seule une enveloppe en brouillon peut être validée.");

        m.Statut = StatutPrevisionMensuelle.Validee;
        m.ValideeParId = _currentUser.UserId;
        m.DateValidation = DateTime.UtcNow;
        _uow.PrevisionsMensuelles.Update(m);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(TypeActionAudit.Validation, nameof(PrevisionMensuelle), id.ToString(), ct: ct);
        return Result.Success();
    }

    public async Task<Result> RefuserAsync(long id, string motif, CancellationToken ct = default)
    {
        var m = await _uow.PrevisionsMensuelles.GetByIdAsync(id, ct);
        if (m is null) return Result.Failure("Enveloppe mensuelle introuvable.");
        if (m.Statut == StatutPrevisionMensuelle.Cloturee)
            return Result.Failure("Une enveloppe clôturée ne peut plus être refusée.");

        // Une enveloppe ouverte peut être annulée tant qu'elle n'a rien financé :
        // c'est le seul recours quand on s'est trompé de mois à la création, sinon
        // son montant resterait à jamais décompté du budget du projet.
        // Dès qu'un décaissement s'y rattache, elle devient un fait comptable.
        if (m.MontantConsomme > 0)
            return Result.Failure(
                $"Cette enveloppe a déjà financé {m.MontantConsomme:N0} Ar de dépenses : " +
                "elle ne peut plus être annulée. Clôturez-la, le reliquat sera reporté.");

        m.Statut = StatutPrevisionMensuelle.Refusee;
        m.MotifRefus = motif;
        _uow.PrevisionsMensuelles.Update(m);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(TypeActionAudit.Refus, nameof(PrevisionMensuelle), id.ToString(),
            nouvelleValeur: motif, ct: ct);
        return Result.Success();
    }

    public async Task<Result<decimal>> CloturerAsync(long id, CancellationToken ct = default)
    {
        var m = await _uow.PrevisionsMensuelles.GetByIdAsync(id, ct);
        if (m is null) return Result<decimal>.Failure("Enveloppe mensuelle introuvable.");
        if (m.Statut != StatutPrevisionMensuelle.Validee)
            return Result<decimal>.Failure("Seule une enveloppe ouverte peut être clôturée.");

        // Aucune journée ne doit rester en cours : sinon le reliquat calculé serait faux.
        var journeeOuverte = await _uow.Previsions.Query().AsNoTracking()
            .Where(p => p.PrevisionMensuelleId == id
                        && p.Statut != StatutPrevision.Cloturee
                        && p.Statut != StatutPrevision.Refusee)
            .OrderBy(p => p.DatePrevision)
            .FirstOrDefaultAsync(ct);

        if (journeeOuverte is not null)
            return Result<decimal>.Failure(
                $"La prévision du {journeeOuverte.DatePrevision:dd/MM/yyyy} ({journeeOuverte.Reference}) " +
                "n'est pas encore clôturée. Réceptionnez ses travaux avant de fermer le mois.");

        var reliquat = m.EnveloppeTotale - m.MontantConsomme;
        if (reliquat < 0) reliquat = 0;

        await _uow.BeginTransactionAsync(ct);
        try
        {
            m.Statut = StatutPrevisionMensuelle.Cloturee;
            m.DateCloture = DateTime.UtcNow;
            m.ClotureeParId = _currentUser.UserId;
            _uow.PrevisionsMensuelles.Update(m);

            // Si le mois suivant a déjà été préparé, on lui pousse le report tout de suite.
            var anneeSuivante = m.Mois == 12 ? m.Annee + 1 : m.Annee;
            var moisSuivant = m.Mois == 12 ? 1 : m.Mois + 1;

            var suivant = await _uow.PrevisionsMensuelles.Query()
                .FirstOrDefaultAsync(x => x.ChantierId == m.ChantierId
                                          && x.Annee == anneeSuivante && x.Mois == moisSuivant
                                          && x.Statut != StatutPrevisionMensuelle.Refusee, ct);

            if (suivant is not null)
            {
                suivant.ReportMoisPrecedent = reliquat;
                suivant.PrevisionMensuellePrecedenteId = m.Id;
                _uow.PrevisionsMensuelles.Update(suivant);
            }
            // Sinon le report sera repris automatiquement à la création du mois suivant.

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync(ct);
            _logger.LogError(ex, "Échec de clôture de l'enveloppe {Id}.", id);
            return Result<decimal>.Failure("Erreur lors de la clôture : " + ex.Message);
        }

        await _audit.LogAsync(TypeActionAudit.Validation, nameof(PrevisionMensuelle), id.ToString(),
            nouvelleValeur: $"Clôturée — reliquat reporté {reliquat:N0} Ar", ct: ct);

        _logger.LogInformation("Enveloppe {Ref} clôturée, reliquat {Reliquat} Ar reporté.", m.Reference, reliquat);
        return Result<decimal>.Success(reliquat);
    }

    public async Task<PrevisionMensuelle?> ObtenirMoisOuvertAsync(long chantierId, DateTime date, CancellationToken ct = default)
        => await _uow.PrevisionsMensuelles.Query()
            .FirstOrDefaultAsync(m => m.ChantierId == chantierId
                                      && m.Annee == date.Year && m.Mois == date.Month
                                      && m.Statut == StatutPrevisionMensuelle.Validee, ct);

    public async Task<decimal> TotalMoisEngagesAsync(long chantierId, long? saufId = null, CancellationToken ct = default)
        => await _uow.PrevisionsMensuelles.Query().AsNoTracking()
            .Where(m => m.ChantierId == chantierId
                        && m.Statut != StatutPrevisionMensuelle.Refusee
                        && (saufId == null || m.Id != saufId))
            .SumAsync(m => (decimal?)m.MontantPrevu, ct) ?? 0m;
}
