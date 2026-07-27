using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

[Authorize]
public class MateriauxController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IAlerteService _alertes;
    private readonly IReferenceDataCache _referenceData;

    public MateriauxController(IUnitOfWork uow, IAlerteService alertes, IReferenceDataCache referenceData)
    {
        _uow = uow;
        _alertes = alertes;
        _referenceData = referenceData;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var mouvements = await _uow.MouvementsMateriau.Query().AsNoTracking()
            .Include(mm => mm.Materiau).ThenInclude(m => m.Chantier)
            .OrderBy(mm => mm.Materiau.Chantier!.Nom)
            .ThenBy(mm => mm.Materiau.Designation)
            .ThenByDescending(mm => mm.DateMouvement)
            .ToListAsync(ct);
        return View(mouvements);
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Materiaux = await _uow.Materiaux.Query().AsNoTracking()
            .Include(m => m.Chantier)
            .OrderBy(m => m.Chantier!.Nom).ThenBy(m => m.Designation)
            .ToListAsync(ct);
        return View(new MouvementMateriau());
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MouvementMateriau model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Materiaux = await _uow.Materiaux.Query().AsNoTracking()
                .Include(m => m.Chantier)
                .OrderBy(m => m.Chantier!.Nom).ThenBy(m => m.Designation)
                .ToListAsync(ct);
            return View(model);
        }

        var materiau = await _uow.Materiaux.GetByIdAsync(model.MateriauxId, ct);
        if (materiau is null)
        {
            TempData["Error"] = "Matériau non trouvé.";
            return RedirectToAction(nameof(Create));
        }

        // Calculer le nouveau solde basé sur les mouvements précédents
        var dernierMouvement = await _uow.MouvementsMateriau.Query().AsNoTracking()
            .Where(mm => mm.MateriauxId == model.MateriauxId)
            .OrderByDescending(mm => mm.DateMouvement)
            .FirstOrDefaultAsync(ct);

        decimal soldeActuel = dernierMouvement?.SoldeEnStock ?? 0;
        model.SoldeEnStock = soldeActuel + model.QuantiteEntree - model.QuantiteSortie;

        // Vérifier qu'on ne sort pas plus qu'on a en stock
        if (model.SoldeEnStock < 0)
        {
            TempData["Error"] = $"Stock insuffisant. Solde actuel : {soldeActuel} {materiau.Unite}";
            return RedirectToAction(nameof(Create));
        }

        // Mettre à jour les totaux du matériau
        materiau.QuantiteRecue += model.QuantiteEntree;
        materiau.QuantiteUtilisee += model.QuantiteSortie;
        _uow.Materiaux.Update(materiau);

        // Créer le mouvement
        await _uow.MouvementsMateriau.AddAsync(model, ct);
        await _uow.SaveChangesAsync(ct);
        await _alertes.EvaluerAlertesAsync(ct);

        TempData["Success"] = $"Mouvement enregistré pour {materiau.Designation}.";
        return RedirectToAction(nameof(Index));
    }

}
