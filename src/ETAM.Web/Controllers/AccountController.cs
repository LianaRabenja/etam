using ETAM.Application.Interfaces;
using ETAM.Domain.Enums;
using ETAM.Infrastructure.Identity;
using ETAM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETAM.Web.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _audit;
    private readonly IEmailSender _email;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IAuditService audit,
        IEmailSender email)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _audit = audit;
        _email = email;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            await _audit.LogAsync(TypeActionAudit.Connexion);
            return RedirectToLocal(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Identifiants invalides ou compte verrouillé.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _audit.LogAsync(TypeActionAudit.Deconnexion);
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var lien = Url.Action(nameof(ResetPassword), "Account",
                new { email = user.Email, token }, protocol: Request.Scheme);

            var corps = $@"<p>Bonjour,</p>
<p>Vous avez demandé la réinitialisation de votre mot de passe ETAM ERP.</p>
<p><a href='{lien}'>Cliquez ici pour définir un nouveau mot de passe</a></p>
<p>Si vous n'êtes pas à l'origine de cette demande, ignorez cet email.</p>
<p>— ETAM ERP</p>";

            await _email.EnvoyerAsync(user.Email!, "Réinitialisation de votre mot de passe ETAM", corps);
        }
        TempData["Message"] = "Si l'adresse existe, un email de réinitialisation a été envoyé.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPassword(string? email, string? token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            return RedirectToAction(nameof(Login));
        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is not null)
        {
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["Message"] = "Mot de passe réinitialisé. Vous pouvez vous connecter.";
                return RedirectToAction(nameof(Login));
            }
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Demande invalide.");
        }
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    // --- Changer son propre mot de passe (utilisateur connecté) ---

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        var result = await _userManager.ChangePasswordAsync(user, model.AncienMotDePasse, model.NouveauMotDePasse);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        // Réémet le cookie d'authentification avec le nouveau jeton de sécurité.
        await _signInManager.RefreshSignInAsync(user);
        await _audit.LogAsync(TypeActionAudit.Modification, "Utilisateur", user.Id,
            nouvelleValeur: "Mot de passe modifié");

        TempData["Success"] = "Votre mot de passe a été modifié.";
        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
        => Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction("Index", "Home");
}
