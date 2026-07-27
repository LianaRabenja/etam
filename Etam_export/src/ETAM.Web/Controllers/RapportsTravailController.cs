using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using ETAM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

/// <summary>
/// Rapport de travail : rapport hebdomadaire d'avancement des travaux, saisi par le Correspondant
/// pour chaque chantier (mêmes libellés que le rapport papier historique), puis soumis à la
/// validation de l'Administrateur.
/// </summary>
[Authorize]
public class RapportsTravailController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IReferenceDataCache _referenceData;

    public RapportsTravailController(IUnitOfWork uow, IReferenceDataCache referenceData)
    {
        _uow = uow;
        _referenceData = referenceData;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var rapports = await _uow.RapportsTravail.Query().AsNoTracking()
            .Include(r => r.Chantier)
            .OrderByDescending(r => r.PeriodeFin)
            .Take(200)
            .ToListAsync(ct);
        return View(rapports);
    }

    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var rapport = await _uow.RapportsTravail.Query().AsNoTracking().AsSplitQuery()
            .Include(r => r.Chantier)
            .Include(r => r.LignesAvancement)
            .Include(r => r.LignesMateriaux)
            .Include(r => r.LignesEquipements)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rapport is null) return NotFound();
        return View(rapport);
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        return View(new RapportTravailFormViewModel
        {
            PeriodeDebut = DateTime.UtcNow.Date.AddDays(-6),
            PeriodeFin = DateTime.UtcNow.Date
        });
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RapportTravailFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            return View(vm);
        }

        var rapport = MapVersEntite(vm, new RapportTravail());
        await _uow.RapportsTravail.AddAsync(rapport, ct);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Rapport de travail enregistré en brouillon.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var r = await _uow.RapportsTravail.Query().AsNoTracking().AsSplitQuery()
            .Include(x => x.LignesAvancement).Include(x => x.LignesMateriaux).Include(x => x.LignesEquipements)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r is null) return NotFound();
        if (!r.EstModifiable)
        {
            TempData["Error"] = "Ce rapport est soumis ou validé : il n'est plus modifiable.";
            return RedirectToAction(nameof(Details), new { id });
        }

        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        ViewBag.RapportId = id;
        return View(MapVersViewModel(r));
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, RapportTravailFormViewModel vm, CancellationToken ct)
    {
        var r = await _uow.RapportsTravail.Query().AsSplitQuery()
            .Include(x => x.LignesAvancement).Include(x => x.LignesMateriaux).Include(x => x.LignesEquipements)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r is null) return NotFound();
        if (!r.EstModifiable)
        {
            TempData["Error"] = "Ce rapport n'est plus modifiable.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            ViewBag.RapportId = id;
            return View(vm);
        }

        foreach (var l in r.LignesAvancement.ToList()) _uow.RapportTravailLignesAvancement.Remove(l);
        foreach (var l in r.LignesMateriaux.ToList()) _uow.RapportTravailLignesMateriaux.Remove(l);
        foreach (var l in r.LignesEquipements.ToList()) _uow.RapportTravailLignesEquipements.Remove(l);

        MapVersEntite(vm, r);
        _uow.RapportsTravail.Update(r);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Rapport de travail modifié.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // --- Workflow de validation ---
    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Soumettre(long id, CancellationToken ct)
    {
        var r = await _uow.RapportsTravail.GetByIdAsync(id, ct);
        if (r is null) return NotFound();
        if (r.Statut != StatutRapportTravail.Brouillon && r.Statut != StatutRapportTravail.Refuse)
        {
            TempData["Error"] = "Ce rapport a déjà été soumis.";
            return RedirectToAction(nameof(Index));
        }
        r.Statut = StatutRapportTravail.Soumis;
        r.SoumisPar = User.Identity?.Name;
        r.DateSoumission = DateTime.UtcNow;
        _uow.RapportsTravail.Update(r);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Rapport soumis pour validation.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Valider(long id, CancellationToken ct)
    {
        var r = await _uow.RapportsTravail.GetByIdAsync(id, ct);
        if (r is null) return NotFound();
        if (r.Statut != StatutRapportTravail.Soumis)
        {
            TempData["Error"] = "Ce rapport n'est pas en attente de validation.";
            return RedirectToAction(nameof(Index));
        }
        r.Statut = StatutRapportTravail.Valide;
        r.ValideParId = User.Identity?.Name;
        r.DateValidation = DateTime.UtcNow;
        _uow.RapportsTravail.Update(r);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Rapport de travail validé.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refuser(long id, string motif, CancellationToken ct)
    {
        var r = await _uow.RapportsTravail.GetByIdAsync(id, ct);
        if (r is null) return NotFound();
        r.Statut = StatutRapportTravail.Refuse;
        r.MotifRefus = string.IsNullOrWhiteSpace(motif) ? "Non précisé" : motif;
        _uow.RapportsTravail.Update(r);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Rapport refusé ; il redevient modifiable par le Correspondant.";
        return RedirectToAction(nameof(Index));
    }

    // --- Mapping ViewModel <-> Entité ---
    private static RapportTravail MapVersEntite(RapportTravailFormViewModel vm, RapportTravail r)
    {
        r.ChantierId = vm.ChantierId;
        r.Numero = vm.Numero;
        r.PeriodeDebut = vm.PeriodeDebut;
        r.PeriodeFin = vm.PeriodeFin;
        r.Lieu = vm.Lieu;
        r.EntrepriseExecutante = vm.EntrepriseExecutante;
        r.ConducteurTravaux = vm.ConducteurTravaux;
        r.EffectifCadres = vm.EffectifCadres;
        r.EffectifOuvriers = vm.EffectifOuvriers;
        r.HoraireMatin = vm.HoraireMatin;
        r.HoraireApresMidi = vm.HoraireApresMidi;
        r.ConditionsMeteo = vm.ConditionsMeteo;
        r.ResumeSuiviPlanning = vm.ResumeSuiviPlanning;
        r.ProblemesRencontres = vm.ProblemesRencontres;
        r.Suggestions = vm.Suggestions;

        r.LignesAvancement = (vm.LignesAvancement ?? new()).Select(l => new RapportTravailAvancementLigne
        {
            Zone = l.Zone, TravauxRealises = l.TravauxRealises,
            NiveauAvancement = l.NiveauAvancement, Observations = l.Observations
        }).ToList();

        r.LignesMateriaux = (vm.LignesMateriaux ?? new()).Select(l => new RapportTravailMateriauLigne
        {
            Materiau = l.Materiau, Unite = l.Unite, QuantiteUtilisee = l.QuantiteUtilisee,
            StockInitial = l.StockInitial, Entree = l.Entree, StockRestant = l.StockRestant,
            Observations = l.Observations
        }).ToList();

        r.LignesEquipements = (vm.LignesEquipements ?? new()).Select(l => new RapportTravailEquipementLigne
        {
            Equipement = l.Equipement, Etat = l.Etat, Observation = l.Observation
        }).ToList();

        return r;
    }

    private static RapportTravailFormViewModel MapVersViewModel(RapportTravail r) => new()
    {
        ChantierId = r.ChantierId,
        Numero = r.Numero,
        PeriodeDebut = r.PeriodeDebut,
        PeriodeFin = r.PeriodeFin,
        Lieu = r.Lieu,
        EntrepriseExecutante = r.EntrepriseExecutante,
        ConducteurTravaux = r.ConducteurTravaux,
        EffectifCadres = r.EffectifCadres,
        EffectifOuvriers = r.EffectifOuvriers,
        HoraireMatin = r.HoraireMatin,
        HoraireApresMidi = r.HoraireApresMidi,
        ConditionsMeteo = r.ConditionsMeteo,
        ResumeSuiviPlanning = r.ResumeSuiviPlanning,
        ProblemesRencontres = r.ProblemesRencontres,
        Suggestions = r.Suggestions,
        LignesAvancement = r.LignesAvancement.Select(l => new RapportTravailAvancementLigneViewModel
        { Zone = l.Zone, TravauxRealises = l.TravauxRealises, NiveauAvancement = l.NiveauAvancement, Observations = l.Observations }).ToList(),
        LignesMateriaux = r.LignesMateriaux.Select(l => new RapportTravailMateriauLigneViewModel
        { Materiau = l.Materiau, Unite = l.Unite, QuantiteUtilisee = l.QuantiteUtilisee, StockInitial = l.StockInitial, Entree = l.Entree, StockRestant = l.StockRestant, Observations = l.Observations }).ToList(),
        LignesEquipements = r.LignesEquipements.Select(l => new RapportTravailEquipementLigneViewModel
        { Equipement = l.Equipement, Etat = l.Etat, Observation = l.Observation }).ToList()
    };
}
