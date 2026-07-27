using ETAM.Domain.Entities;
using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETAM.Web.Controllers;

/// <summary>
/// Catalogue des désignations avec prix de référence. Alimente l'autocomplétion
/// des approvisionnements et pré-remplit le prix unitaire.
/// </summary>
[Authorize(Roles = "Administrateur,Correspondant")]
public class CatalogueController : Controller
{
    private readonly IUnitOfWork _uow;

    public CatalogueController(IUnitOfWork uow) => _uow = uow;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var articles = (await _uow.Catalogue.ListAllAsync(ct))
            .OrderBy(a => a.Categorie).ThenBy(a => a.Designation).ToList();
        return View(articles);
    }

    [HttpGet]
    public IActionResult Create() => View(new ArticleCatalogue());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ArticleCatalogue model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        if (await _uow.Catalogue.AnyAsync(a => a.Designation == model.Designation, ct))
        {
            ModelState.AddModelError(nameof(model.Designation), "Cette désignation existe déjà dans le catalogue.");
            return View(model);
        }
        await _uow.Catalogue.AddAsync(model, ct);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"« {model.Designation} » ajouté au catalogue.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var article = await _uow.Catalogue.GetByIdAsync(id, ct);
        if (article is null) return NotFound();
        return View(article);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ArticleCatalogue model, CancellationToken ct)
    {
        var article = await _uow.Catalogue.GetByIdAsync(model.Id, ct);
        if (article is null) return NotFound();
        if (!ModelState.IsValid) return View(model);

        article.Designation = model.Designation;
        article.Categorie = model.Categorie;
        article.Unite = model.Unite;
        article.PrixUnitaire = model.PrixUnitaire;
        _uow.Catalogue.Update(article);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Article mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var article = await _uow.Catalogue.GetByIdAsync(id, ct);
        if (article is not null)
        {
            _uow.Catalogue.Remove(article);
            await _uow.SaveChangesAsync(ct);
            TempData["Success"] = "Article retiré du catalogue.";
        }
        return RedirectToAction(nameof(Index));
    }
}
