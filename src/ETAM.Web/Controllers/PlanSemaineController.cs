using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

/// <summary>
/// Le plan de la semaine : ce qu'on prévoit de dépenser chaque jour, à l'intérieur
/// de l'enveloppe du mois. Ce n'est qu'un plan — aucun argent n'y est engagé.
/// Les demandes du chantier viennent ensuite s'y comparer.
/// </summary>
[Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
public class PlanSemaineController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IReferenceDataCache _referenceData;
    private readonly IAuditService _audit;

    public PlanSemaineController(IUnitOfWork uow, IReferenceDataCache referenceData, IAuditService audit)
    {
        _uow = uow;
        _referenceData = referenceData;
        _audit = audit;
    }

    /// <summary>Grille des sept jours d'une semaine, pour un chantier.</summary>
    public async Task<IActionResult> Index(long? chantierId, DateTime? jour, CancellationToken ct)
    {
        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);

        var reference = (jour ?? DateTime.UtcNow).Date;

        // Recule jusqu'au lundi. Le cast en int est indispensable : le modulo
        // n'existe pas sur DayOfWeek. Le décalage de 6 place dimanche en fin de
        // semaine plutôt qu'au début, comme en France et à Madagascar.
        var decalage = ((int)reference.DayOfWeek + 6) % 7;
        var lundi = DateTime.SpecifyKind(reference.AddDays(-decalage), DateTimeKind.Utc);
        var dimanche = lundi.AddDays(6);

        ViewBag.Lundi = lundi;
        ViewBag.ChantierId = chantierId;

        if (chantierId is null)
            return View(new List<JourDeLaSemaine>());

        // L'enveloppe du mois qui couvre cette semaine.
        var enveloppe = await _uow.PrevisionsMensuelles.Query().AsNoTracking()
            .FirstOrDefaultAsync(m => m.ChantierId == chantierId
                                      && m.Annee == lundi.Year && m.Mois == lundi.Month
                                      && m.Statut == StatutPrevisionMensuelle.Validee, ct);
        ViewBag.Enveloppe = enveloppe;

        var plans = await _uow.PlansJournaliers.Query().AsNoTracking()
            .Where(p => p.ChantierId == chantierId && p.Date >= lundi && p.Date <= dimanche)
            .ToListAsync(ct);

        // Ce que le chantier a effectivement demandé sur chaque jour.
        var demandes = await _uow.Previsions.Query().AsNoTracking()
            .Where(p => p.ChantierId == chantierId
                        && p.DatePrevision >= lundi && p.DatePrevision <= dimanche
                        && p.Statut != StatutPrevision.Refusee)
            .Select(p => new
            {
                p.Id,
                p.DatePrevision,
                p.Statut,
                Montant = p.Lignes.Where(l => !l.IsDeleted).Sum(l => l.Quantite * l.PrixUnitaireEstime)
            })
            .ToListAsync(ct);

        var semaine = new List<JourDeLaSemaine>();
        for (var i = 0; i < 7; i++)
        {
            var d = lundi.AddDays(i);
            var plan = plans.FirstOrDefault(p => p.Date.Date == d.Date);
            var dem = demandes.Where(x => x.DatePrevision.Date == d.Date).ToList();

            semaine.Add(new JourDeLaSemaine(
                d,
                PlanJournalier.NomDuJour(d.DayOfWeek),
                plan?.Id,
                plan?.MontantPrevu ?? 0m,
                plan?.Observation,
                dem.Sum(x => x.Montant),
                dem.Count,
                dem.FirstOrDefault()?.Id));
        }

        return View(semaine);
    }

    /// <summary>Enregistre les sept montants d'un coup.</summary>
    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enregistrer(
        long chantierId, DateTime lundi, DateTime[] dates, decimal[] montants, string[] observations,
        CancellationToken ct)
    {
        var enveloppe = await _uow.PrevisionsMensuelles.Query()
            .FirstOrDefaultAsync(m => m.ChantierId == chantierId
                                      && m.Annee == lundi.Year && m.Mois == lundi.Month
                                      && m.Statut == StatutPrevisionMensuelle.Validee, ct);

        if (enveloppe is null)
        {
            TempData["Error"] = "Aucune enveloppe ouverte pour ce mois. " +
                                "Créez et validez l'enveloppe du mois avant de planifier la semaine.";
            return RedirectToAction(nameof(Index), new { chantierId, jour = lundi });
        }

        // Le total planifié sur le mois ne doit pas dépasser l'enveloppe.
        var debutMois = new DateTime(enveloppe.Annee, enveloppe.Mois, 1, 0, 0, 0, DateTimeKind.Utc);
        var finMois = debutMois.AddMonths(1).AddDays(-1);

        var dejaPlanifie = await _uow.PlansJournaliers.Query().AsNoTracking()
            .Where(p => p.ChantierId == chantierId
                        && p.Date >= debutMois && p.Date <= finMois
                        && (p.Date < lundi || p.Date > lundi.AddDays(6)))
            .SumAsync(p => (decimal?)p.MontantPrevu, ct) ?? 0m;

        var totalSemaine = montants?.Sum() ?? 0m;

        if (dejaPlanifie + totalSemaine > enveloppe.EnveloppeTotale)
        {
            TempData["Error"] =
                $"Le plan dépasse l'enveloppe de {enveloppe.Libelle} : " +
                $"{dejaPlanifie:N0} Ar déjà planifiés sur les autres semaines, " +
                $"{totalSemaine:N0} Ar pour celle-ci, " +
                $"pour une enveloppe de {enveloppe.EnveloppeTotale:N0} Ar.";
            return RedirectToAction(nameof(Index), new { chantierId, jour = lundi });
        }

        for (var i = 0; i < (dates?.Length ?? 0); i++)
        {
            var date = DateTime.SpecifyKind(dates![i].Date, DateTimeKind.Utc);
            var montant = i < montants!.Length ? montants[i] : 0m;
            var note = observations is not null && i < observations.Length ? observations[i] : null;

            var existant = await _uow.PlansJournaliers.Query()
                .FirstOrDefaultAsync(p => p.ChantierId == chantierId && p.Date == date, ct);

            if (montant <= 0)
            {
                // Un montant remis à zéro efface la journée du plan.
                if (existant is not null)
                {
                    existant.IsDeleted = true;
                    _uow.PlansJournaliers.Update(existant);
                }
                continue;
            }

            if (existant is null)
            {
                await _uow.PlansJournaliers.AddAsync(new PlanJournalier
                {
                    PrevisionMensuelleId = enveloppe.Id,
                    ChantierId = chantierId,
                    Date = date,
                    MontantPrevu = montant,
                    Observation = note
                }, ct);
            }
            else
            {
                existant.PrevisionMensuelleId = enveloppe.Id;
                existant.MontantPrevu = montant;
                existant.Observation = note;
                _uow.PlansJournaliers.Update(existant);
            }
        }

        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync(TypeActionAudit.Modification, nameof(PlanJournalier), chantierId.ToString(),
            nouvelleValeur: $"Semaine du {lundi:dd/MM/yyyy} — {totalSemaine:N0} Ar planifiés", ct: ct);

        TempData["Success"] = $"Plan de la semaine enregistré : {totalSemaine:N0} Ar prévus.";
        return RedirectToAction(nameof(Index), new { chantierId, jour = lundi });
    }
}

/// <summary>Une case de la grille hebdomadaire.</summary>
public record JourDeLaSemaine(
    DateTime Date,
    string NomDuJour,
    long? PlanId,
    decimal MontantPrevu,
    string? Observation,
    decimal MontantDemande,
    int NombreDemandes,
    long? PremiereDemandeId)
{
    public decimal Ecart => MontantDemande - MontantPrevu;
    public bool RienDemande => NombreDemandes == 0;
}
