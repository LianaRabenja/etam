using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETAM.Web.Controllers;

[Authorize(Roles = "Administrateur")]
public class ParametresController : Controller
{
    private readonly IUnitOfWork _uow;

    public ParametresController(IUnitOfWork uow) => _uow = uow;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var parametres = (await _uow.Parametres.ListAllAsync(ct))
            .OrderBy(p => p.Groupe).ThenBy(p => p.Cle).ToList();
        return View(parametres);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enregistrer(long id, string? valeur, CancellationToken ct)
    {
        var p = await _uow.Parametres.GetByIdAsync(id, ct);
        if (p is not null)
        {
            p.Valeur = valeur;
            _uow.Parametres.Update(p);
            await _uow.SaveChangesAsync(ct);
            TempData["Success"] = $"Paramètre « {p.Cle} » mis à jour.";
        }
        return RedirectToAction(nameof(Index));
    }
}
