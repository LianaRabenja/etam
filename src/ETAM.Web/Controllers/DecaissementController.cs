using ETAM.Application.Interfaces;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

/// <summary>
/// Décaissements : les sorties d'argent réelles à l'intérieur d'une prévision
/// journalière. Chaque enregistrement débite un compte bancaire.
/// </summary>
[Authorize]
public class DecaissementController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IDecaissementService _service;

    public DecaissementController(IUnitOfWork uow, IDecaissementService service)
    {
        _uow = uow;
        _service = service;
    }

    /// <summary>
    /// Journal des sorties d'argent. Deux natures d'écritures cohabitent :
    ///
    ///   REMISE   — l'argent quitte la banque à l'exécution d'une prévision.
    ///              Générée automatiquement, une par prévision ouverte.
    ///   PAIEMENT — l'argent remis est distribué à quelqu'un. Saisie à la main.
    ///
    /// Seuls les paiements consomment le plafond du jour : une remise n'est pas
    /// une dépense, c'est un déplacement de la banque vers les mains du chef.
    /// C'est ce qui permet au reliquat de se reporter au lendemain.
    /// </summary>
    public async Task<IActionResult> Index(long? previsionId, CancellationToken ct)
    {
        // --- Les remises : une par prévision ouverte ---
        var remisesQuery = _uow.Previsions.Query().AsNoTracking()
            .Where(p => p.Statut == StatutPrevision.Executee
                        || p.Statut == StatutPrevision.RapportSoumis
                        || p.Statut == StatutPrevision.Cloturee);

        if (previsionId.HasValue)
            remisesQuery = remisesQuery.Where(p => p.Id == previsionId.Value);

        var remises = await remisesQuery
            .OrderByDescending(p => p.DateExecution)
            .Take(300)
            .Select(p => new LigneSortie(
                p.DateExecution ?? p.DatePrevision,
                p.Chantier.Nom,
                p.Reference,
                p.Id,
                true,
                p.Chantier.Responsable ?? "Chef de chantier",
                "Remise de l'enveloppe du " + p.DatePrevision.ToString("dd/MM/yyyy"),
                p.Lignes.Where(l => !l.IsDeleted).Sum(l => l.Quantite * l.PrixUnitaireEstime),
                null,
                p.AccuseNomSignataire))
            .ToListAsync(ct);

        // --- Les paiements détaillés ---
        var paiementsQuery = _uow.Decaissements.Query().AsNoTracking().AsQueryable();
        if (previsionId.HasValue)
            paiementsQuery = paiementsQuery.Where(d => d.PrevisionJournaliereId == previsionId.Value);

        var paiements = await paiementsQuery
            .OrderByDescending(d => d.Date)
            .Take(300)
            .Select(d => new LigneSortie(
                d.Date,
                d.PrevisionJournaliere.Chantier.Nom,
                d.PrevisionJournaliere.Reference,
                d.PrevisionJournaliereId,
                false,
                d.Beneficiaire,
                d.Motif,
                d.Montant,
                d.Mode,
                d.AccuseNom))
            .ToListAsync(ct);

        var liste = remises.Concat(paiements)
            .OrderByDescending(l => l.Date)
            .ThenByDescending(l => l.EstRemise)   // la remise d'abord, puis ses paiements
            .ToList();

        ViewBag.PrevisionId = previsionId;
        ViewBag.TotalRemis = remises.Sum(r => r.Montant);
        ViewBag.TotalDistribue = paiements.Sum(p => p.Montant);
        return View(liste);
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpGet]
    public async Task<IActionResult> Create(long previsionId, CancellationToken ct)
    {
        var p = await _uow.Previsions.Query().AsNoTracking()
            .Include(x => x.Chantier)
            .Include(x => x.Lignes)
            .FirstOrDefaultAsync(x => x.Id == previsionId, ct);
        if (p is null) return NotFound();

        if (p.Statut != StatutPrevision.Executee)
        {
            TempData["Error"] = "Cette prévision n'est pas ouverte : aucun décaissement possible.";
            return RedirectToAction("Details", "Prevision", new { id = previsionId });
        }

        if (!p.DateAccuseReception.HasValue)
        {
            TempData["Error"] = "Le chef de chantier doit d'abord accuser réception de l'argent.";
            return RedirectToAction("Details", "Prevision", new { id = previsionId });
        }

        await ChargerListesAsync(p.ChantierId, ct);
        ViewBag.Prevision = p;
        return View();
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DecaissementDto dto, CancellationToken ct)
    {
        var res = await _service.EnregistrerAsync(dto, ct);
        if (!res.Succeeded)
        {
            TempData["Error"] = res.Error;
            var p = await _uow.Previsions.Query().AsNoTracking()
                .Include(x => x.Chantier).Include(x => x.Lignes)
                .FirstOrDefaultAsync(x => x.Id == dto.PrevisionJournaliereId, ct);
            if (p is null) return NotFound();
            await ChargerListesAsync(p.ChantierId, ct);
            ViewBag.Prevision = p;
            return View(dto);
        }

        TempData["Success"] = $"Décaissement de {dto.Montant:N0} Ar enregistré. " +
                              "Vous pouvez y joindre la facture.";
        return RedirectToAction("Details", "Prevision", new { id = dto.PrevisionJournaliereId });
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Annuler(long id, string motif, long previsionId, CancellationToken ct)
    {
        var res = await _service.AnnulerAsync(id, motif, ct);
        if (res.Succeeded) TempData["Success"] = "Décaissement annulé et argent recrédité.";
        else TempData["Error"] = res.Error;
        return RedirectToAction("Details", "Prevision", new { id = previsionId });
    }

    private async Task ChargerListesAsync(long chantierId, CancellationToken ct)
    {
        // Comptes du chantier + compte général de l'entreprise.
        // On projette dans un record public et non dans un type anonyme : ces derniers
        // sont internes à l'assembly du contrôleur et la liaison dynamique échouerait
        // au moment du rendu de la vue.
        var comptes = await _uow.ComptesBancaires.Query().AsNoTracking()
            .Where(c => c.EstActif && (c.ChantierId == chantierId || c.ChantierId == null))
            .OrderBy(c => c.Nom)
            .Select(c => new { c.Id, c.Nom, c.Banque, c.Solde })
            .ToListAsync(ct);

        ViewBag.Comptes = comptes
            .Select(c => new OptionCompte(c.Id, $"{c.Nom} — {c.Banque} ({c.Solde:N0} Ar)"))
            .ToList();
    }
}

/// <summary>Entrée de la liste déroulante des comptes débitables.</summary>
public record OptionCompte(long Id, string Libelle);

/// <summary>
/// Ligne du journal des sorties d'argent, qu'il s'agisse d'une remise d'enveloppe
/// (générée à l'exécution) ou d'un paiement détaillé (saisi à la main).
/// </summary>
public record LigneSortie(
    DateTime Date,
    string Chantier,
    string Reference,
    long PrevisionId,
    bool EstRemise,
    string Beneficiaire,
    string Motif,
    decimal Montant,
    ModePaiement? Mode,
    string? Signataire);
