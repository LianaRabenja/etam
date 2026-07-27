using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETAM.Web.Controllers;

[Authorize]
public class BudgetComptesController : Controller
{
    private readonly IBudgetService _budgetService;
    private readonly IUnitOfWork _uow;

    public BudgetComptesController(IBudgetService budgetService, IUnitOfWork uow)
    {
        _budgetService = budgetService;
        _uow = uow;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var budgets = (await _uow.BudgetsComptes.ListAllAsync(ct))
            .OrderByDescending(b => b.Annee).ToList();
        return View(budgets);
    }

    [Authorize(Roles = "Administrateur")]
    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var budget = await _uow.BudgetsComptes.GetByIdAsync(id, ct);
        if (budget is null) return NotFound();
        return View(budget);
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BudgetCompte model, CancellationToken ct)
    {
        var budget = await _uow.BudgetsComptes.GetByIdAsync(model.Id, ct);
        if (budget is null) return NotFound();

        budget.Libelle = model.Libelle;
        budget.MontantInitial = model.MontantInitial;
        budget.Reserve = model.Reserve;
        budget.EstActif = model.EstActif;
        _uow.BudgetsComptes.Update(budget);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Budget mis à jour.";
        return RedirectToAction(nameof(Index));
    }
}
