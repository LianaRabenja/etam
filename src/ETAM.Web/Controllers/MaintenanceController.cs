using ETAM.Domain.Interfaces;
using ETAM.Infrastructure.Identity;
using ETAM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

/// <summary>
/// Remise à zéro de la base, pour la phase de tests.
///
/// CE QUE FAIT LE BOUTON : il vide toutes les tables métier et garde les comptes
/// utilisateurs, leurs rôles, l'historique des migrations et les clés de
/// protection des données. Les compteurs d'identifiants repartent à 1.
///
/// IL EST VOLONTAIREMENT DIFFICILE À DÉCLENCHER. Quatre verrous :
///
///   1. Rôle Administrateur exigé.
///   2. Un interrupteur de configuration (Maintenance:ReinitialisationAutorisee).
///      Tant qu'il n'est pas à true, l'écran répond 404 — il n'existe pas.
///      C'est lui qu'on coupera le jour où la base portera de vrais clients.
///   3. La phrase « EFFACER TOUT » doit être saisie à l'identique.
///   4. Le mot de passe du compte connecté doit être ressaisi.
///
/// Un bouton capable d'effacer une base ne doit jamais être à un seul clic
/// d'un geste ordinaire. Une erreur ici n'a pas d'annulation.
/// </summary>
[Authorize(Roles = "Administrateur")]
public class MaintenanceController : Controller
{
    /// <summary>La phrase exacte que l'Administrateur doit recopier.</summary>
    private const string PhraseConfirmation = "EFFACER TOUT";

    /// <summary>
    /// Les tables vidées. Listées une par une, SANS « CASCADE », et c'est capital :
    /// CASCADE se propage aux tables qui RÉFÉRENCENT celles qu'on vide, et
    /// AspNetUsers porte une colonne ChantierId. Un CASCADE effacerait donc les
    /// comptes utilisateurs — exactement ce qu'on veut garder. Sans CASCADE,
    /// une table oubliée fait ÉCHOUER l'opération au lieu de l'élargir.
    /// </summary>
    private static readonly string[] TablesMetier =
    {
        "Chantiers", "BudgetsComptes",
        "PrevisionsGlobales", "PrevisionsGlobalesLignes",
        "PrevisionsMensuelles", "PrevisionMensuelleLignes", "PlansJournaliers",
        "Previsions", "PrevisionLignes", "AutresDepensesJour",
        "Decaissements", "PiecesJointes",
        "Approvisionnements", "ApprovisionnementLignes",
        "RapportsTravail", "RapportTravailLignesAvancement",
        "RapportTravailLignesMateriaux", "RapportTravailLignesEquipements",
        "ComptesBancaires", "MouvementsBancaires", "Banques",
        "Materiaux", "MouvementsMateriau", "Fournisseurs", "DettesFournisseurs",
        "Catalogue", "Depenses", "Alertes", "AuditLogs", "Parametres"
    };

    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IConfiguration _config;
    private readonly ILogger<MaintenanceController> _logger;

    public MaintenanceController(
        ApplicationDbContext context,
        IUnitOfWork uow,
        UserManager<ApplicationUser> users,
        IConfiguration config,
        ILogger<MaintenanceController> logger)
    {
        _context = context;
        _uow = uow;
        _users = users;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// L'interrupteur de configuration. Absent ou false : l'écran n'existe pas.
    /// Sur Render, se pose en variable d'environnement :
    ///   Maintenance__ReinitialisationAutorisee = true
    /// (deux tirets bas — c'est la notation ASP.NET pour une section).
    /// </summary>
    private bool Autorisee =>
        _config.GetValue<bool>("Maintenance:ReinitialisationAutorisee");

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        // 404 plutôt que 403 : tant que l'interrupteur est coupé, cette page
        // n'a pas à révéler qu'elle existe.
        if (!Autorisee) return NotFound();

        await ChargerCompteursAsync(ct);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reinitialiser(
        string confirmation, string motDePasse, CancellationToken ct)
    {
        if (!Autorisee) return NotFound();

        // --- Verrou 3 : la phrase, à l'identique ---
        if (!string.Equals(confirmation?.Trim(), PhraseConfirmation, StringComparison.Ordinal))
        {
            TempData["Error"] = $"La phrase de confirmation ne correspond pas. "
                                + $"Recopiez exactement : {PhraseConfirmation}";
            return RedirectToAction(nameof(Index));
        }

        // --- Verrou 4 : le mot de passe du compte connecté ---
        var utilisateur = await _users.GetUserAsync(User);
        if (utilisateur is null)
        {
            TempData["Error"] = "Session expirée. Reconnectez-vous.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrEmpty(motDePasse)
            || !await _users.CheckPasswordAsync(utilisateur, motDePasse))
        {
            _logger.LogWarning(
                "Réinitialisation refusée : mot de passe incorrect pour {User}.",
                utilisateur.UserName);
            TempData["Error"] = "Mot de passe incorrect. Rien n'a été effacé.";
            return RedirectToAction(nameof(Index));
        }

        // La trace part dans les logs AVANT l'effacement : la table AuditLogs
        // fait partie de ce qui est vidé, y écrire ne servirait à rien.
        _logger.LogWarning(
            "RÉINITIALISATION DE LA BASE demandée par {User} ({Email}) depuis {Ip}.",
            utilisateur.UserName, utilisateur.Email,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "inconnue");

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            // Détacher les utilisateurs de leur chantier : sans cela la clé
            // étrangère ChantierId bloque le vidage de la table Chantiers.
            await _context.Database.ExecuteSqlRawAsync(
                @"UPDATE ""AspNetUsers"" SET ""ChantierId"" = NULL WHERE ""ChantierId"" IS NOT NULL;",
                ct);

            // Les noms viennent d'un tableau constant du code, jamais d'une
            // saisie : aucune valeur extérieure n'entre dans cette requête.
            var liste = string.Join(", ", TablesMetier.Select(t => $"\"{t}\""));
            await _context.Database.ExecuteSqlRawAsync(
                $"TRUNCATE TABLE {liste} RESTART IDENTITY;", ct);

            await transaction.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(ex, "Échec de la réinitialisation de la base.");
            TempData["Error"] =
                "La réinitialisation a échoué et rien n'a été effacé : " + ex.Message;
            return RedirectToAction(nameof(Index));
        }

        _logger.LogWarning("Base réinitialisée par {User}.", utilisateur.UserName);

        TempData["Success"] =
            "Base réinitialisée. Les comptes utilisateurs sont conservés, mais ils ne sont "
            + "plus rattachés à un chantier. Commencez par créer une banque, puis un chantier.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Ce qui sera effacé, compté avant le geste.</summary>
    private async Task ChargerCompteursAsync(CancellationToken ct)
    {
        ViewBag.NbUtilisateurs = await _users.Users.CountAsync(ct);
        ViewBag.NbChantiers = await _uow.Chantiers.CountAsync(ct: ct);
        ViewBag.NbPrevisions = await _uow.Previsions.CountAsync(ct: ct);
        ViewBag.NbMouvements = await _uow.MouvementsBancaires.CountAsync(ct: ct);
        ViewBag.NbComptes = await _uow.ComptesBancaires.CountAsync(ct: ct);
        ViewBag.NbTables = TablesMetier.Length;
        ViewBag.Phrase = PhraseConfirmation;
    }
}
