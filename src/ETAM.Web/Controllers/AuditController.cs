using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

[Authorize(Roles = "Administrateur")]
public class AuditController : Controller
{
    private readonly IUnitOfWork _uow;

    public AuditController(IUnitOfWork uow) => _uow = uow;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var logs = (await _uow.AuditLogs.Query().AsNoTracking()
            .OrderByDescending(a => a.DateAction).Take(500)
            .ToListAsync(ct));
        return View(logs);
    }
}
