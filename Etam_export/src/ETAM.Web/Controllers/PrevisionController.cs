using ETAM.Application.DTOs;
using ETAM.Application.Interfaces;
using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

[Authorize]
public class PrevisionController : Controller
{
    private readonly IPrevisionService _service;
    private readonly IUnitOfWork _uow;
    private readonly IReferenceDataCache _referenceData;

    public PrevisionController(IPrevisionService service, IUnitOfWork uow, IReferenceDataCache referenceData)
    {
        _service = service;
        _uow = uow;
        _referenceData = referenceData;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var previsions = await _uow.Previsions.Query().AsNoTracking()
            .Include(p => p.Chantier)
            .Include(p => p.Lignes)
            .OrderByDescending(p => p.DatePrevision)
            .Take(200)
            .ToListAsync(ct);
        return View(previsions);
    }

    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var prevision = await _uow.Previsions.Query().AsNoTracking()
            .Include(p => p.Chantier)
            .Include(p => p.Lignes)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (prevision is null) return NotFound();

        // Contexte budgétaire pour aider le valideur à décider.
        var budget = (await _uow.BudgetsComptes.ListAsync(b => b.EstActif, ct))
            .OrderByDescending(b => b.Annee).FirstOrDefault();
        ViewBag.BudgetCompteRestant = budget?.MontantRestant ?? 0m;
        ViewBag.BudgetMaterielRestant = prevision.Chantier?.BudgetMaterielRestant ?? 0m;
        return View(prevision);
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        await ChargerDettesAsync(ct);
        await ChargerCatalogueAsync(ct);
        return View(new PrevisionCreateDto());
    }

    // Dettes non soldées, pour les lignes de remboursement de dette.
    private async Task ChargerDettesAsync(CancellationToken ct)
    {
        ViewBag.Dettes = await _uow.DettesFournisseurs.Query()
            .Include(d => d.Fournisseur)
            .Where(d => d.Statut != ETAM.Domain.Enums.StatutDette.Soldee)
            .ToListAsync(ct);
    }

    // Charge le catalogue (désignations + prix) pour l'autocomplétion et le pré-remplissage du prix.
    private async Task ChargerCatalogueAsync(CancellationToken ct)
    {
        ViewBag.Catalogue = (await _uow.Catalogue.ListAllAsync(ct))
            .OrderBy(a => a.Designation).ToList();

        var predefinies = new[] { "Nourriture", "Carburant", "Eau", "Transport", "Gros œuvre",
            "Ferraillage", "Forage", "Bois", "Consommable", "Location", "Déplacement", "Réparation" };
        var catCatalogue = await _uow.Catalogue.Query().Select(a => a.Categorie).ToListAsync(ct);
        ViewBag.Categories = catCatalogue.Concat(predefinies)
            .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
    }

    [Authorize(Roles = "Administrateur,Chef de chantier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PrevisionCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            await ChargerDettesAsync(ct);
            await ChargerCatalogueAsync(ct);
            return View(dto);
        }
        var result = await _service.CreerAsync(dto, ct);
        if (!result.Succeeded) { TempData["Error"] = result.Error; }
        else TempData["Success"] = "Prévision créée en brouillon.";
        return RedirectToAction(nameof(Index));
    }

    // --- Modification d'une prévision non encore validée par l'Administrateur ---
    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var p = await _uow.Previsions.Query().AsNoTracking()
            .Include(x => x.Lignes).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();
        if (!p.EstModifiable)
        {
            TempData["Error"] = "Cette prévision est validée par l'Administrateur : elle n'est plus modifiable.";
            return RedirectToAction(nameof(Details), new { id });
        }

        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        await ChargerDettesAsync(ct);
        await ChargerCatalogueAsync(ct);
        ViewBag.PrevId = id;
        var dto = new PrevisionCreateDto
        {
            ChantierId = p.ChantierId,
            DatePrevision = p.DatePrevision,
            Observation = p.Observation,
            Lignes = p.Lignes.Select(l => new PrevisionLigneCreateDto
            {
                Designation = l.Designation, Categorie = l.Categorie, TypeBudget = l.TypeBudget,
                MateriauId = l.MateriauId, DetteFournisseurId = l.DetteFournisseurId,
                Quantite = l.Quantite, PrixUnitaireEstime = l.PrixUnitaireEstime, Observation = l.Observation
            }).ToList()
        };
        return View(dto);
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, PrevisionCreateDto dto, CancellationToken ct)
    {
        var p = await _uow.Previsions.Query()
            .Include(x => x.Lignes).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();
        if (!p.EstModifiable)
        {
            TempData["Error"] = "Cette prévision n'est plus modifiable.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (dto.Lignes is null || dto.Lignes.Count == 0)
            ModelState.AddModelError(string.Empty, "Ajoutez au moins une ligne.");
        if (!ModelState.IsValid)
        {
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            await ChargerDettesAsync(ct);
            await ChargerCatalogueAsync(ct);
            ViewBag.PrevId = id;
            return View(dto);
        }

        p.ChantierId = dto.ChantierId;
        p.DatePrevision = dto.DatePrevision;
        p.Observation = dto.Observation;

        foreach (var ancienne in p.Lignes.ToList())
            _uow.PrevisionLignes.Remove(ancienne);
        p.Lignes = dto.Lignes.Select(l => new ETAM.Domain.Entities.PrevisionLigne
        {
            Designation = l.Designation, Categorie = l.Categorie, TypeBudget = l.TypeBudget,
            MateriauId = l.MateriauId, DetteFournisseurId = l.DetteFournisseurId,
            Quantite = l.Quantite, PrixUnitaireEstime = l.PrixUnitaireEstime, Observation = l.Observation
        }).ToList();

        _uow.Previsions.Update(p);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Prévision modifiée.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Soumettre(long id, CancellationToken ct)
        => Retour(await _service.SoumettreAsync(id, ct));

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValiderRf(long id, CancellationToken ct)
        => Retour(await _service.ValiderResponsableFinancierAsync(id, ct));

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValiderAdmin(long id, CancellationToken ct)
        => Retour(await _service.ValiderAdministrateurAsync(id, ct));

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Executer(long id, bool utiliserReserve, CancellationToken ct)
        => Retour(await _service.ExecuterAsync(id, utiliserReserve, ct));

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refuser(long id, string motif, CancellationToken ct)
        => Retour(await _service.RefuserAsync(id, motif ?? "Non précisé", ct));

    private IActionResult Retour(ETAM.Application.Common.Models.Result r)
    {
        if (r.Succeeded) TempData["Success"] = "Opération effectuée.";
        else TempData["Error"] = r.Error;
        return RedirectToAction(nameof(Index));
    }
}
