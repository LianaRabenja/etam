using ETAM.Infrastructure.Identity;
using ETAM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

[Authorize(Roles = "Administrateur")]
public class UtilisateursController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UtilisateursController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var users = await _userManager.Users.ToListAsync(ct);
        var roles = new Dictionary<string, string>();
        foreach (var u in users)
            roles[u.Id] = string.Join(", ", await _userManager.GetRolesAsync(u));
        ViewBag.Roles = roles;
        return View(users);
    }

    // --- Création d'un utilisateur avec attribution de rôle ---
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Roles = RolesEtam.Tous;
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = RolesEtam.Tous;
            return View(model);
        }

        if (await _userManager.FindByEmailAsync(model.Email) is not null)
        {
            ModelState.AddModelError(string.Empty, "Un utilisateur avec cet email existe déjà.");
            ViewBag.Roles = RolesEtam.Tous;
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            NomComplet = model.NomComplet,
            Fonction = model.Fonction,
            EstActif = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            ViewBag.Roles = RolesEtam.Tous;
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, model.Role);
        TempData["Success"] = $"Utilisateur « {model.NomComplet} » créé avec le rôle {model.Role}.";
        return RedirectToAction(nameof(Index));
    }

    // --- Gestion du rôle d'un utilisateur ---
    [HttpGet]
    public async Task<IActionResult> GererRoles(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        ViewBag.Roles = RolesEtam.Tous;
        ViewBag.RoleActuel = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GererRoles(string id, string role)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (!await _roleManager.RoleExistsAsync(role))
        {
            TempData["Error"] = "Rôle inconnu.";
            return RedirectToAction(nameof(Index));
        }

        var rolesActuels = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, rolesActuels);
        await _userManager.AddToRoleAsync(user, role);

        TempData["Success"] = $"Rôle de « {user.NomComplet} » changé en {role}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BasculerActivation(string id, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is not null)
        {
            user.EstActif = !user.EstActif;
            // Verrouillage effectif si désactivé.
            user.LockoutEnd = user.EstActif ? null : DateTimeOffset.MaxValue;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = $"Utilisateur {(user.EstActif ? "activé" : "désactivé")}.";
        }
        return RedirectToAction(nameof(Index));
    }
}
