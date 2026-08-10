using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

/// <summary>
/// Justificatifs numérisés : factures d'achat, reçus, bons de livraison.
/// Le contenu est stocké dans PostgreSQL et non sur le disque du serveur, qui est
/// recréé à chaque mise à jour de l'application.
/// </summary>
[Authorize]
public class PieceJointeController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Taille maximale acceptée par fichier.</summary>
    private const long TailleMax = 5 * 1024 * 1024;   // 5 Mo

    /// <summary>Nombre maximal de pièces par prévision, pour ne pas saturer la base.</summary>
    private const int NombreMaxParPrevision = 20;

    private static readonly string[] TypesAutorises =
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp", "image/heic", "application/pdf"
    };

    public PieceJointeController(IUnitOfWork uow, IAuditService audit, ICurrentUserService currentUser)
    {
        _uow = uow;
        _audit = audit;
        _currentUser = currentUser;
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier,Magasinier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<IActionResult> Televerser(
        long? previsionId, long? decaissementId, long? rapportTravailId,
        List<IFormFile> fichiers, string? description, string? numeroPiece,
        string? emetteur, decimal? montantFacture, CancellationToken ct)
    {
        if (previsionId is null && decaissementId is null && rapportTravailId is null)
        {
            TempData["Error"] = "Aucun élément de rattachement fourni.";
            return Redirect(Request.Headers.Referer.ToString());
        }

        if (fichiers is null || fichiers.Count == 0)
        {
            TempData["Error"] = "Aucun fichier sélectionné.";
            return RetourVers(previsionId, decaissementId, rapportTravailId);
        }

        if (previsionId.HasValue)
        {
            var deja = await _uow.PiecesJointes.CountAsync(p => p.PrevisionJournaliereId == previsionId.Value, ct);
            if (deja + fichiers.Count > NombreMaxParPrevision)
            {
                TempData["Error"] = $"Maximum {NombreMaxParPrevision} pièces par prévision " +
                                    $"({deja} déjà présentes).";
                return RetourVers(previsionId, decaissementId, rapportTravailId);
            }
        }

        var ajoutees = 0;
        var refusees = new List<string>();

        foreach (var fichier in fichiers)
        {
            if (fichier.Length == 0) continue;

            if (fichier.Length > TailleMax)
            {
                refusees.Add($"{fichier.FileName} (trop volumineux, maximum 5 Mo)");
                continue;
            }

            var type = (fichier.ContentType ?? string.Empty).ToLowerInvariant();
            if (!TypesAutorises.Contains(type))
            {
                refusees.Add($"{fichier.FileName} (format non accepté : photo ou PDF uniquement)");
                continue;
            }

            using var flux = new MemoryStream();
            await fichier.CopyToAsync(flux, ct);

            await _uow.PiecesJointes.AddAsync(new PieceJointe
            {
                PrevisionJournaliereId = previsionId,
                DecaissementId = decaissementId,
                RapportTravailId = rapportTravailId,
                NomFichier = Path.GetFileName(fichier.FileName),
                TypeMime = type,
                Taille = fichier.Length,
                Contenu = flux.ToArray(),
                Description = description,
                NumeroPiece = numeroPiece,
                Emetteur = emetteur,
                MontantFacture = montantFacture,
                DateAjout = DateTime.UtcNow,
                AjouteParId = _currentUser.UserId
            }, ct);

            ajoutees++;
        }

        if (ajoutees > 0)
        {
            await _uow.SaveChangesAsync(ct);
            await _audit.LogAsync(TypeActionAudit.Ajout, nameof(PieceJointe),
                (previsionId ?? decaissementId ?? rapportTravailId)?.ToString(),
                nouvelleValeur: $"{ajoutees} justificatif(s) ajouté(s)", ct: ct);

            TempData["Success"] = ajoutees == 1
                ? "Justificatif ajouté."
                : $"{ajoutees} justificatifs ajoutés.";
        }

        if (refusees.Count > 0)
            TempData["Error"] = "Non ajouté(s) : " + string.Join(" ; ", refusees);

        return RetourVers(previsionId, decaissementId, rapportTravailId);
    }

    /// <summary>Affiche le fichier dans le navigateur (photo ou PDF).</summary>
    public async Task<IActionResult> Voir(long id, CancellationToken ct)
    {
        var p = await _uow.PiecesJointes.Query().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();

        Response.Headers.CacheControl = "private, max-age=3600";
        return File(p.Contenu, p.TypeMime);
    }

    /// <summary>Force le téléchargement du fichier.</summary>
    public async Task<IActionResult> Telecharger(long id, CancellationToken ct)
    {
        var p = await _uow.PiecesJointes.Query().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();

        return File(p.Contenu, p.TypeMime, p.NomFichier);
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Supprimer(long id, CancellationToken ct)
    {
        var p = await _uow.PiecesJointes.GetByIdAsync(id, ct);
        if (p is null) return NotFound();

        p.IsDeleted = true;
        _uow.PiecesJointes.Update(p);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(TypeActionAudit.Suppression, nameof(PieceJointe), id.ToString(),
            ancienneValeur: p.NomFichier, ct: ct);

        TempData["Success"] = "Justificatif retiré.";
        return RetourVers(p.PrevisionJournaliereId, p.DecaissementId, p.RapportTravailId);
    }

    private IActionResult RetourVers(long? previsionId, long? decaissementId, long? rapportTravailId)
    {
        if (previsionId.HasValue)
            return RedirectToAction("Details", "Prevision", new { id = previsionId.Value });
        if (rapportTravailId.HasValue)
            return RedirectToAction("Details", "RapportTravail", new { id = rapportTravailId.Value });
        if (decaissementId.HasValue)
            return RedirectToAction("Index", "Decaissement");
        return RedirectToAction("Index", "Home");
    }
}
