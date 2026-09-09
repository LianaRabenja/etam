using ETAM.Domain.Entities;
using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETAM.Web.Controllers;

[Authorize(Roles = "Administrateur,Correspondant")]
public class FournisseursController : Controller
{
    private readonly IUnitOfWork _uow;

    public FournisseursController(IUnitOfWork uow) => _uow = uow;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var fournisseurs = (await _uow.Fournisseurs.ListAllAsync(ct)).OrderBy(f => f.Nom).ToList();
        return View(fournisseurs);
    }

    [HttpGet]
    public IActionResult Create() => View(new Fournisseur());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Fournisseur model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        await _uow.Fournisseurs.AddAsync(model, ct);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"Fournisseur « {model.Nom} » créé.";
        return RedirectToAction(nameof(Index));
    }
}
