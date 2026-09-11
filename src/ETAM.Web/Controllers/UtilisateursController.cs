using ETAM.Domain.Interfaces;
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
    private readonly IUnitOfWork _uow;

    public UtilisateursController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUnitOfWork uow)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _uow = uow;
    }

    private async Task ChargerListesAsync(CancellationToken ct = default)
    {
        ViewBag.Roles = RolesEtam.Tous;
        ViewBag.Chantiers = await _uow.Chantiers.Query().AsNoTracking()
            .OrderBy(c => c.Nom).ToListAsync(ct);
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var users = await _userManager.Users.ToListAsync(ct);
        var roles = new Dictionary<string, string>();
        foreach (var u in users)
            roles[u.Id] = string.Join(", ", await _userManager.GetRolesAsync(u));
        ViewBag.Roles = roles;
        // Nom du chantier d'affectation : c'est lui qui détermine ce que chacun voit.
        ViewBag.NomsChantiers = await _uow.Chantiers.Query().AsNoTracking()
            .ToDictionaryAsync(c => c.Id, c => c.Nom, ct);
        return View(users);
    }

    // --- Création d'un utilisateur avec attribution de rôle ---
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await ChargerListesAsync(ct);
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model, CancellationToken ct)
    {
        // Un magasinier est forcément rattaché à un chantier (il n'en gère qu'un seul).
        if (model.Role == RolesEtam.Magasinier && model.ChantierId is null or 0)
            ModelState.AddModelError(nameof(model.ChantierId),
                "Un magasinier doit être affecté à un chantier.");

        if (!ModelState.IsValid)
        {
            await ChargerListesAsync(ct);
            return View(model);
        }

        if (await _userManager.FindByEmailAsync(model.Email) is not null)
        {
            ModelState.AddModelError(string.Empty, "Un utilisateur avec cet email existe déjà.");
            await ChargerListesAsync(ct);
            return View(model);
        }

        // Un Magasinier comme un Chef de chantier travaillent sur UN chantier : sans
        // affectation, ils verraient tous les chantiers. Le rattachement est obligatoire.
        var rolesCloisonnes = new[] { "Magasinier", "Chef de chantier" };
        if (rolesCloisonnes.Contains(model.Role) && model.ChantierId is not > 0)
        {
            ModelState.AddModelError(nameof(model.ChantierId),
                $"Un {model.Role} doit être rattaché à un chantier : il ne verra que celui-ci.");
            await ChargerListesAsync(ct);
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            NomComplet = model.NomComplet,
            Fonction = model.Fonction,
            EstActif = true,
            ChantierId = model.ChantierId is > 0 ? model.ChantierId : null
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            await ChargerListesAsync(ct);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, model.Role);
        TempData["Success"] = $"Utilisateur « {model.NomComplet} » créé avec le rôle {model.Role}.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Change le chantier d'affectation d'un utilisateur (magasinier, chef de chantier).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AffecterChantier(string id, long? chantierId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        user.ChantierId = chantierId is > 0 ? chantierId : null;
        await _userManager.UpdateAsync(user);

        var nom = user.ChantierId is null
            ? "tous les chantiers"
            : (await _uow.Chantiers.GetByIdAsync(user.ChantierId.Value, ct))?.Nom ?? "—";
        TempData["Success"] = $"{user.NomComplet} est désormais affecté à : {nom}.";
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

        if (user is not null && user.UserName == User.Identity?.Name)
        {
            TempData["Error"] = "Vous ne pouvez pas désactiver votre propre compte.";
            return RedirectToAction(nameof(Index));
        }

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

    /// <summary>Nombre d'administrateurs actifs : empêche de se retrouver sans aucun accès total.</summary>
    private async Task<int> NombreAdministrateursAsync()
        => (await _userManager.GetUsersInRoleAsync(RolesEtam.Administrateur)).Count;

    // --- Modification d'un utilisateur ---
    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        await ChargerListesAsync(ct);
        var vm = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email ?? "",
            NomComplet = user.NomComplet ?? "",
            Fonction = user.Fonction,
            Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "",
            ChantierId = user.ChantierId,
            EstActif = user.EstActif
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user is null) return NotFound();

        // Magasinier et Chef de chantier travaillent sur UN chantier : sans affectation,
        // les filtres ne s'appliquent pas et ils verraient tous les chantiers.
        var rolesCloisonnes = new[] { RolesEtam.Magasinier, RolesEtam.ChefDeChantier };
        if (rolesCloisonnes.Contains(model.Role) && model.ChantierId is not > 0)
            ModelState.AddModelError(nameof(model.ChantierId),
                $"Un {model.Role} doit être rattaché à un chantier : il ne verra que celui-ci.");

        // Email déjà pris par quelqu'un d'autre ?
        var autre = await _userManager.FindByEmailAsync(model.Email);
        if (autre is not null && autre.Id != user.Id)
            ModelState.AddModelError(nameof(model.Email), "Cet email est déjà utilisé par un autre compte.");

        var roleActuel = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

        // Ne pas supprimer le dernier administrateur en lui changeant son rôle.
        if (roleActuel == RolesEtam.Administrateur && model.Role != RolesEtam.Administrateur
            && await NombreAdministrateursAsync() <= 1)
            ModelState.AddModelError(nameof(model.Role),
                "C'est le dernier administrateur : changez d'abord le rôle d'un autre compte en Administrateur.");

        // Ni se retirer à soi-même l'accès.
        if (user.UserName == User.Identity?.Name && !model.EstActif)
            ModelState.AddModelError(nameof(model.EstActif), "Vous ne pouvez pas désactiver votre propre compte.");

        if (!ModelState.IsValid)
        {
            await ChargerListesAsync(ct);
            return View(model);
        }

        user.NomComplet = model.NomComplet;
        user.Fonction = model.Fonction;
        user.ChantierId = model.ChantierId is > 0 ? model.ChantierId : null;
        user.EstActif = model.EstActif;
        user.LockoutEnd = model.EstActif ? null : DateTimeOffset.MaxValue;

        if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
        {
            user.Email = model.Email;
            user.UserName = model.Email;   // la connexion se fait par email
        }

        var maj = await _userManager.UpdateAsync(user);
        if (!maj.Succeeded)
        {
            foreach (var e in maj.Errors) ModelState.AddModelError(string.Empty, e.Description);
            await ChargerListesAsync(ct);
            return View(model);
        }

        if (roleActuel != model.Role)
        {
            var rolesActuels = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, rolesActuels);
            await _userManager.AddToRoleAsync(user, model.Role);
        }

        // Mot de passe : uniquement s'il a été saisi.
        if (!string.IsNullOrWhiteSpace(model.NouveauMotDePasse))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await _userManager.ResetPasswordAsync(user, token, model.NouveauMotDePasse);
            if (!reset.Succeeded)
            {
                foreach (var e in reset.Errors) ModelState.AddModelError(string.Empty, e.Description);
                await ChargerListesAsync(ct);
                return View(model);
            }
            TempData["Success"] = $"« {user.NomComplet} » mis à jour, mot de passe réinitialisé.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = $"« {user.NomComplet} » mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    // --- Suppression définitive ---
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Supprimer(string id, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (user.UserName == User.Identity?.Name)
        {
            TempData["Error"] = "Vous ne pouvez pas supprimer votre propre compte.";
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains(RolesEtam.Administrateur) && await NombreAdministrateursAsync() <= 1)
        {
            TempData["Error"] = "Impossible de supprimer le dernier administrateur.";
            return RedirectToAction(nameof(Index));
        }

        var nom = user.NomComplet;
        var result = await _userManager.DeleteAsync(user);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? $"Utilisateur « {nom} » supprimé."
            : "Suppression impossible : " + string.Join(" ", result.Errors.Select(e => e.Description));

        return RedirectToAction(nameof(Index));
    }
}
