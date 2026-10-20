using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using ETAM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

[Authorize]
public class AlertesController : Controller
{
    private readonly IUnitOfWork _uow;

    public AlertesController(IUnitOfWork uow) => _uow = uow;

    /// <summary>
    /// Les alertes que CET utilisateur a le droit de voir. Toutes les lectures et
    /// tous les marquages passent par ici : sans cela, un Chef de chantier pouvait
    /// marquer comme lue une alerte de budget destinée à l'Administrateur, qui ne
    /// la voyait alors jamais.
    /// </summary>
    private IQueryable<ETAM.Domain.Entities.Alerte> AlertesVisibles()
    {
        var q = _uow.Alertes.Query();
        var types = VisibiliteAlertes.TypesVisibles(User);
        return types is null ? q : q.Where(a => types.Contains(a.Type));
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var alertes = await AlertesVisibles().AsNoTracking()
            .Include(a => a.Chantier)
            .OrderByDescending(a => a.CreatedAt).Take(200)
            .ToListAsync(ct);
        return View(alertes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarquerLue(long id, CancellationToken ct)
    {
        // On relit l'alerte dans le périmètre de l'utilisateur : une alerte qu'il
        // ne voit pas, il ne peut pas la marquer non plus, même en forçant l'id.
        var a = await AlertesVisibles().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (a is not null && !a.EstLue)
        {
            a.EstLue = true;
            a.DateLecture = DateTime.UtcNow;
            _uow.Alertes.Update(a);
            await _uow.SaveChangesAsync(ct);
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var restantes = await AlertesVisibles().CountAsync(x => !x.EstLue, ct);
            return Json(new { success = true, id, restantes });
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarquerToutesLues(CancellationToken ct)
    {
        var nonLues = await AlertesVisibles().Where(a => !a.EstLue).ToListAsync(ct);
        foreach (var a in nonLues)
        {
            a.EstLue = true;
            a.DateLecture = DateTime.UtcNow;
            _uow.Alertes.Update(a);
        }
        await _uow.SaveChangesAsync(ct);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success = true, restantes = 0 });
        return RedirectToAction(nameof(Index));
    }
}
