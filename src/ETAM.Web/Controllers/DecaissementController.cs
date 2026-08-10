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

    /// <summary>Journal des décaissements, tous chantiers confondus.</summary>
    public async Task<IActionResult> Index(long? previsionId, CancellationToken ct)
    {
        var q = _uow.Decaissements.Query().AsNoTracking()
            .Include(d => d.PrevisionJournaliere).ThenInclude(p => p.Chantier)
            .Include(d => d.CompteBancaire)
            .AsQueryable();

        if (previsionId.HasValue)
            q = q.Where(d => d.PrevisionJournaliereId == previsionId.Value);

        var liste = await q.OrderByDescending(d => d.Date).ThenByDescending(d => d.Id)
            .Take(300).ToListAsync(ct);

        ViewBag.PrevisionId = previsionId;
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
