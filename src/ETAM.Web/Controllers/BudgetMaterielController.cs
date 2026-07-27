using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETAM.Web.Controllers;

[Authorize]
public class BudgetMaterielController : Controller
{
    private readonly IUnitOfWork _uow;

    public BudgetMaterielController(IUnitOfWork uow) => _uow = uow;

    // Budget Matériel global = somme des budgets matériels des chantiers.
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var chantiers = (await _uow.Chantiers.ListAllAsync(ct))
            .OrderBy(c => c.Nom).ToList();
        return View(chantiers);
    }
}
