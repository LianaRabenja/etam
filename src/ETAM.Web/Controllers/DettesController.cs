using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

[Authorize(Roles = "Administrateur,Correspondant")]
public class DettesController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IBanqueService _banque;
    private readonly IReferenceDataCache _referenceData;

    public DettesController(IUnitOfWork uow, IBanqueService banque, IReferenceDataCache referenceData)
    {
        _uow = uow;
        _banque = banque;
        _referenceData = referenceData;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var dettes = await _uow.DettesFournisseurs.Query().AsNoTracking()
            .Include(d => d.Fournisseur)
            .Include(d => d.Chantier)
            .OrderByDescending(d => d.CreatedAt).ToListAsync(ct);
        return View(dettes);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Fournisseurs = await _uow.Fournisseurs.ListAllAsync(ct);
        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        return View(new DetteFournisseur());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DetteFournisseur model, CancellationToken ct)
    {
        // Même piège que pour les matériaux : DetteFournisseur.Fournisseur est une
        // navigation non nullable, donc réputée obligatoire par la validation, alors
        // que le formulaire n'envoie que FournisseurId. Sans ces deux lignes, aucune
        // dette ne pouvait être enregistrée — en silence.
        ModelState.Remove(nameof(DetteFournisseur.Fournisseur));
        ModelState.Remove(nameof(DetteFournisseur.Chantier));

        if (!ModelState.IsValid)
        {
            ViewBag.Fournisseurs = await _uow.Fournisseurs.ListAllAsync(ct);
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            return View(model);
        }
        await _uow.DettesFournisseurs.AddAsync(model, ct);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Dette enregistrée.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Payer(long id, CancellationToken ct)
    {
        var dette = await _uow.DettesFournisseurs.Query().AsNoTracking()
            .Include(d => d.Fournisseur).Include(d => d.Chantier)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
        if (dette is null) return NotFound();
        ViewBag.Comptes = await _uow.ComptesBancaires.ListAllAsync(ct);
        return View(dette);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Payer(long id, long compteId, decimal montant, bool genererDepense, CancellationToken ct)
    {
        var result = await _banque.PayerDetteAsync(id, compteId, montant, genererDepense, ct);
        if (result.Succeeded) TempData["Success"] = "Paiement enregistré (banque + dette mis à jour).";
        else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Index));
    }
}
