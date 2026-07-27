using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

[Authorize]
public class DepensesController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IReferenceDataCache _referenceData;

    public DepensesController(IUnitOfWork uow, IReferenceDataCache referenceData)
    {
        _uow = uow;
        _referenceData = referenceData;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var depenses = await _uow.Depenses.Query().AsNoTracking()
            .Include(d => d.Chantier)
            .OrderByDescending(d => d.Date).Take(300)
            .ToListAsync(ct);
        return View(depenses);
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        return View(new Depense { Date = DateTime.UtcNow });
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Depense model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            return View(model);
        }

        await _uow.Depenses.AddAsync(model, ct);

        // Impact automatique sur le budget concerné.
        if (model.BudgetConcerne == TypeBudget.Materiel)
        {
            var chantier = await _uow.Chantiers.GetByIdAsync(model.ChantierId, ct);
            if (chantier is not null)
            {
                chantier.Consommation += model.Montant;
                _uow.Chantiers.Update(chantier);
            }
        }
        else
        {
            var budget = (await _uow.BudgetsComptes.ListAsync(b => b.EstActif, ct))
                .OrderByDescending(b => b.Annee).FirstOrDefault();
            if (budget is not null)
            {
                budget.MontantConsomme += model.Montant;
                _uow.BudgetsComptes.Update(budget);
            }
        }

        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"Dépense de {model.Montant:N0} Ar enregistrée.";
        return RedirectToAction(nameof(Index));
    }
}
