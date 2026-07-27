using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

[Authorize]
public class AlertesController : Controller
{
    private readonly IUnitOfWork _uow;

    public AlertesController(IUnitOfWork uow) => _uow = uow;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var alertes = await _uow.Alertes.Query().AsNoTracking()
            .Include(a => a.Chantier)
            .OrderByDescending(a => a.CreatedAt).Take(200)
            .ToListAsync(ct);
        return View(alertes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarquerLue(long id, CancellationToken ct)
    {
        var a = await _uow.Alertes.GetByIdAsync(id, ct);
        if (a is not null && !a.EstLue)
        {
            a.EstLue = true;
            a.DateLecture = DateTime.UtcNow;
            _uow.Alertes.Update(a);
            await _uow.SaveChangesAsync(ct);
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var restantes = await _uow.Alertes.CountAsync(x => !x.EstLue, ct);
            return Json(new { success = true, id, restantes });
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarquerToutesLues(CancellationToken ct)
    {
        var nonLues = await _uow.Alertes.Query().Where(a => !a.EstLue).ToListAsync(ct);
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
