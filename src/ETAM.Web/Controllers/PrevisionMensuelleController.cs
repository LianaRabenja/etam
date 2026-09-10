using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

/// <summary>
/// Enveloppes mensuelles : le montant alloué à un chantier pour un mois donné,
/// augmenté du reliquat non dépensé du mois précédent.
/// </summary>
[Authorize]
public class PrevisionMensuelleController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IPrevisionMensuelleService _service;
    private readonly IReferenceDataCache _referenceData;

    public PrevisionMensuelleController(
        IUnitOfWork uow,
        IPrevisionMensuelleService service,
        IReferenceDataCache referenceData)
    {
        _uow = uow;
        _service = service;
        _referenceData = referenceData;
    }

    public async Task<IActionResult> Index(long? chantierId, CancellationToken ct)
    {
        var q = _uow.PrevisionsMensuelles.Query().AsNoTracking().Include(m => m.Chantier);
        var liste = await (chantierId.HasValue
                ? q.Where(m => m.ChantierId == chantierId.Value)
                : q)
            .OrderByDescending(m => m.Annee).ThenByDescending(m => m.Mois)
            .Take(120)
            .ToListAsync(ct);

        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        ViewBag.ChantierId = chantierId;
        return View(liste);
    }

    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var m = await _uow.PrevisionsMensuelles.Query().AsNoTracking()
            .Include(x => x.Chantier)
            .Include(x => x.Lignes)
            .Include(x => x.PrevisionMensuellePrecedente)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (m is null) return NotFound();

        // Les journées du mois, avec ce qui a été réellement décaissé sur chacune.
        ViewBag.Journees = await _uow.Previsions.Query().AsNoTracking()
            .Where(p => p.PrevisionMensuelleId == id)
            .OrderBy(p => p.DatePrevision)
            .Select(p => new JourneeDuMois(
                p.Id, p.Reference, p.DatePrevision, p.Statut,
                p.Lignes.Where(l => !l.IsDeleted).Sum(l => l.Quantite * l.PrixUnitaireEstime),
                p.ReportVeille, p.MontantDecaisse, p.DateAccuseReception))
            .ToListAsync(ct);

        return View(m);
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpGet]
    public async Task<IActionResult> Create(long? chantierId, CancellationToken ct)
    {
        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        ViewBag.ChantierId = chantierId;
        var maintenant = DateTime.UtcNow;
        return View(new PrevisionMensuelle
        {
            ChantierId = chantierId ?? 0,
            Annee = maintenant.Year,
            Mois = maintenant.Month
        });
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        long chantierId, int annee, int mois, decimal montantPrevu, string? observation,
        string[]? rubriques, decimal[]? montantsRubriques, CancellationToken ct)
    {
        List<(string, string?, decimal, long?)>? lignes = null;
        if (rubriques is not null && montantsRubriques is not null)
        {
            lignes = new List<(string, string?, decimal, long?)>();
            for (var i = 0; i < rubriques.Length && i < montantsRubriques.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(rubriques[i]) && montantsRubriques[i] > 0)
                    lignes.Add((rubriques[i].Trim(), null, montantsRubriques[i], null));
            }
        }

        var res = await _service.CreerAsync(chantierId, annee, mois, montantPrevu, lignes, observation, ct);
        if (!res.Succeeded)
        {
            TempData["Error"] = res.Error;
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            ViewBag.ChantierId = chantierId;
            return View(new PrevisionMensuelle
            {
                ChantierId = chantierId, Annee = annee, Mois = mois,
                MontantPrevu = montantPrevu, Observation = observation
            });
        }

        TempData["Success"] = "Enveloppe mensuelle créée. Validez-la pour l'ouvrir aux prévisions journalières.";
        return RedirectToAction(nameof(Details), new { id = res.Data });
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Valider(long id, CancellationToken ct)
    {
        var res = await _service.ValiderAsync(id, ct);
        if (res.Succeeded) TempData["Success"] = "Enveloppe ouverte.";
        else TempData["Error"] = res.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refuser(long id, string motif, CancellationToken ct)
    {
        var res = await _service.RefuserAsync(id, motif, ct);
        if (res.Succeeded) TempData["Success"] = "Enveloppe refusée.";
        else TempData["Error"] = res.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cloturer(long id, CancellationToken ct)
    {
        var res = await _service.CloturerAsync(id, ct);
        if (res.Succeeded)
            TempData["Success"] = res.Data > 0
                ? $"Mois clôturé. Reliquat de {res.Data:N0} Ar reporté sur le mois suivant."
                : "Mois clôturé. Aucun reliquat à reporter.";
        else TempData["Error"] = res.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Écran de correction d'une enveloppe : montant, observation ET répartition par
    /// rubrique. Réservé à l'Administrateur, et seulement tant qu'aucune sortie d'argent
    /// ne s'y est imputée — après, le mois est un fait comptable.
    ///
    /// Le chantier, l'année et le mois ne sont pas modifiables : les changer reviendrait
    /// à créer une autre enveloppe, et la base n'en accepte qu'une par chantier et par
    /// mois. Pour changer de mois, supprimez celle-ci et créez la bonne.
    /// </summary>
    [Authorize(Roles = "Administrateur")]
    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var m = await _uow.PrevisionsMensuelles.Query()
            .Include(x => x.Chantier)
            .Include(x => x.Lignes)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (m is null) return NotFound();

        if (m.MontantConsomme > 0)
        {
            TempData["Error"] =
                $"Cette enveloppe a déjà financé {m.MontantConsomme:N0} Ar : elle n'est plus modifiable.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var chantier = await _uow.Chantiers.GetByIdAsync(m.ChantierId, ct);
        ViewBag.BudgetProjet = chantier?.BudgetProjet ?? 0m;
        ViewBag.DejaEngage   = await _service.TotalMoisEngagesAsync(m.ChantierId, m.Id, ct);
        return View(m);
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        long id, decimal montantPrevu, string? observation,
        string[]? rubriques, decimal[]? montantsRubriques, CancellationToken ct)
    {
        var m = await _uow.PrevisionsMensuelles.Query()
            .Include(x => x.Lignes)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (m is null) return NotFound();

        if (m.MontantConsomme > 0)
        {
            TempData["Error"] = $"Cette enveloppe a déjà financé {m.MontantConsomme:N0} Ar : elle n'est plus modifiable.";
            return RedirectToAction(nameof(Details), new { id });
        }
        if (montantPrevu <= 0)
        {
            TempData["Error"] = "Le montant du mois doit être supérieur à zéro.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var chantier = await _uow.Chantiers.GetByIdAsync(m.ChantierId, ct);
        if (chantier is null) return NotFound();

        // On exclut l'enveloppe courante du cumul : sinon on la compterait deux fois.
        var dejaEngage = await _service.TotalMoisEngagesAsync(m.ChantierId, m.Id, ct);
        if (dejaEngage + montantPrevu > chantier.BudgetProjet)
        {
            TempData["Error"] =
                $"Budget projet dépassé : {dejaEngage:N0} Ar déjà répartis sur {chantier.BudgetProjet:N0} Ar. " +
                $"Ce mois ne peut pas dépasser {(chantier.BudgetProjet - dejaEngage):N0} Ar.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var ancien = m.MontantPrevu;
        m.MontantPrevu = montantPrevu;
        m.Observation  = observation;
        _uow.PrevisionsMensuelles.Update(m);

        // La répartition est remplacée en bloc : plus simple et plus sûr que de tenter
        // d'apparier ligne à ligne des rubriques que l'utilisateur peut renommer.
        foreach (var ancienne in await _uow.PrevisionMensuelleLignes.Query()
                     .Where(l => l.PrevisionMensuelleId == id).ToListAsync(ct))
            _uow.PrevisionMensuelleLignes.Remove(ancienne);

        if (rubriques is not null && montantsRubriques is not null)
        {
            for (var i = 0; i < rubriques.Length && i < montantsRubriques.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(rubriques[i]) || montantsRubriques[i] <= 0) continue;
                await _uow.PrevisionMensuelleLignes.AddAsync(new PrevisionMensuelleLigne
                {
                    PrevisionMensuelleId = id,
                    Rubrique = rubriques[i].Trim(),
                    Montant  = montantsRubriques[i]
                }, ct);
            }
        }

        await _uow.SaveChangesAsync(ct);

        TempData["Success"] = ancien == montantPrevu
            ? $"Répartition de {m.Libelle} mise à jour."
            : $"Enveloppe de {m.Libelle} : {ancien:N0} Ar → {montantPrevu:N0} Ar, répartition mise à jour.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Supprime une enveloppe mensuelle. Réservé à l'Administrateur, et seulement si
    /// elle n'a rien financé et qu'aucune prévision journalière ne s'y rattache.
    /// C'est le recours quand on s'est trompé de mois : son montant redevient
    /// disponible pour les autres mois du chantier.
    /// </summary>
    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Supprimer(long id, CancellationToken ct)
    {
        var m = await _uow.PrevisionsMensuelles.GetByIdAsync(id, ct);
        if (m is null) return NotFound();

        if (m.MontantConsomme > 0)
        {
            TempData["Error"] = $"Cette enveloppe a déjà financé {m.MontantConsomme:N0} Ar : elle ne peut pas être supprimée. Clôturez-la.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var nbJours = await _uow.Previsions.Query()
            .CountAsync(p => p.PrevisionMensuelleId == id, ct);
        if (nbJours > 0)
        {
            TempData["Error"] = $"{nbJours} prévision(s) journalière(s) sont rattachées à ce mois. Supprimez-les d'abord.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var libelle = m.Libelle;

        // Un mois suivant peut pointer sur celui-ci pour son report : on coupe le lien.
        var suivant = await _uow.PrevisionsMensuelles.Query()
            .FirstOrDefaultAsync(x => x.PrevisionMensuellePrecedenteId == id, ct);
        if (suivant is not null)
        {
            suivant.PrevisionMensuellePrecedenteId = null;
            suivant.ReportMoisPrecedent = 0;
            _uow.PrevisionsMensuelles.Update(suivant);
        }

        foreach (var pj in await _uow.PlansJournaliers.Query()
                     .Where(x => x.PrevisionMensuelleId == id).ToListAsync(ct))
            _uow.PlansJournaliers.Remove(pj);

        foreach (var l in await _uow.PrevisionMensuelleLignes.Query()
                     .Where(x => x.PrevisionMensuelleId == id).ToListAsync(ct))
            _uow.PrevisionMensuelleLignes.Remove(l);

        _uow.PrevisionsMensuelles.Remove(m);
        await _uow.SaveChangesAsync(ct);

        TempData["Success"] = $"Enveloppe de {libelle} supprimée. Son montant redevient disponible pour les autres mois.";
        return RedirectToAction(nameof(Index));
    }
}

/// <summary>Ligne d'affichage : une journée à l'intérieur d'un mois.</summary>
public record JourneeDuMois(
    long Id, string Reference, DateTime Date, StatutPrevision Statut,
    decimal Demande, decimal ReportVeille, decimal Decaisse, DateTime? DateAccuse)
{
    public decimal Plafond => Demande + ReportVeille;
    public decimal Reliquat => Plafond - Decaisse;
}
