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
[Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
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
}

/// <summary>Ligne d'affichage : une journée à l'intérieur d'un mois.</summary>
public record JourneeDuMois(
    long Id, string Reference, DateTime Date, StatutPrevision Statut,
    decimal Demande, decimal ReportVeille, decimal Decaisse, DateTime? DateAccuse)
{
    public decimal Plafond => Demande + ReportVeille;
    public decimal Reliquat => Plafond - Decaisse;
}
