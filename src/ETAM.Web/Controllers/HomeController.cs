using ETAM.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETAM.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IDashboardService _dashboard;

    public HomeController(IDashboardService dashboard) => _dashboard = dashboard;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        // Seuls l'Administrateur et le Correspondant voient le tableau de bord financier.
        // Les autres rôles (Chef de chantier, Magasinier, ...) arrivent sur une page d'accueil
        // simplifiée avec des accès rapides adaptés à leur rôle.
        if (!User.IsInRole("Administrateur") && !User.IsInRole("Correspondant"))
            return RedirectToAction(nameof(Accueil));

        var model = await _dashboard.ObtenirAsync(ct);
        return View(model);
    }

    /// <summary>Page d'accueil simplifiée (sans le tableau de bord financier) pour les rôles
    /// opérationnels : Chef de chantier, Magasinier, etc.</summary>
    public IActionResult Accueil() => View();

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
