using ETAM.Application.DTOs;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using ETAM.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

[Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
public class PrevisionGlobaleController : Controller
{
    private readonly IUnitOfWork _uow;

    public PrevisionGlobaleController(IUnitOfWork uow) => _uow = uow;

    private async Task ChargerChantiersAsync(CancellationToken ct)
    {
        ViewBag.Chantiers = await _uow.Chantiers.Query().AsNoTracking()
            .OrderBy(c => c.Nom).ToListAsync(ct);

        // Rubriques déjà utilisées (pour l'autocomplétion) + quelques valeurs par défaut.
        var predefinies = new[] { "Approvisionnement", "Main d'œuvre", "Matériel", "Transport",
            "Carburant", "Location", "Sous-traitance", "Divers" };
        var existantes = await _uow.PrevisionsGlobalesLignes.Query().AsNoTracking()
            .Select(l => l.Rubrique).ToListAsync(ct);
        ViewBag.Rubriques = existantes.Concat(predefinies)
            .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var previsions = await _uow.PrevisionsGlobales.Query().AsNoTracking()
            .Include(p => p.Chantier).Include(p => p.Lignes)
            .OrderByDescending(p => p.DateCreation).Take(200).ToListAsync(ct);
        return View(previsions);
    }

    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var prev = await _uow.PrevisionsGlobales.Query().AsNoTracking()
            .Include(p => p.Chantier).Include(p => p.Lignes)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (prev is null) return NotFound();
        return View(prev);
    }

    /// <summary>Export PDF / Excel d'une prévision globale (lignes groupées par rubrique).</summary>
    public async Task<IActionResult> Export(long id, string format, CancellationToken ct)
    {
        var prev = await _uow.PrevisionsGlobales.Query().AsNoTracking()
            .Include(p => p.Chantier).Include(p => p.Lignes)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (prev is null) return NotFound();

        var lignes = prev.Lignes.OrderBy(l => l.Rubrique).ThenBy(l => l.Designation).ToList();
        var cols = new List<ColonneExport<PrevisionGlobaleLigne>>
        {
            new("Rubrique",    l => l.Rubrique),
            new("Désignation", l => l.Designation),
            new("Unité",       l => l.Unite ?? ""),
            new("Quantité",    l => l.Quantite.ToString("N0"), true),
            new("Prix unit.",  l => l.PrixUnitaire.ToString("N0"), true),
            new("Total (Ar)",  l => (l.Quantite * l.PrixUnitaire).ToString("N0"), true)
        };

        var nom = $"PrevisionGlobale_{prev.Reference}".Replace(' ', '_');
        if (format == "excel")
            return File(ExportService.Excel("Prevision globale", lignes, cols),
                ExportService.MimeExcel, ExportService.NomFichier(nom, "xlsx"));

        var budget = prev.Chantier?.BudgetProjet ?? 0m;
        var sousTitre = $"{prev.Chantier?.Nom} · {prev.DateCreation:dd/MM/yyyy} · Statut : {prev.Statut}";
        var pied = $"Total prévu : {prev.Total:N0} Ar" +
                   (budget > 0 ? $" · Budget projet : {budget:N0} Ar · Écart : {budget - prev.Total:N0} Ar" : "");

        return File(ExportService.Pdf("Prévision globale du projet", sousTitre, lignes, cols, pied, paysage: true),
            ExportService.MimePdf, ExportService.NomFichier(nom, "pdf"));
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpGet]
    public async Task<IActionResult> Create(long? chantierId, CancellationToken ct)
    {
        await ChargerChantiersAsync(ct);
        return View(new PrevisionGlobaleCreateDto { ChantierId = chantierId ?? 0 });
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PrevisionGlobaleCreateDto dto, CancellationToken ct)
    {
        if (dto.Lignes is null || dto.Lignes.Count == 0)
            ModelState.AddModelError(string.Empty, "Ajoutez au moins une ligne.");

        if (!ModelState.IsValid)
        {
            await ChargerChantiersAsync(ct);
            return View(dto);
        }

        var chantier = await _uow.Chantiers.GetByIdAsync(dto.ChantierId, ct);
        if (chantier is null) { TempData["Error"] = "Chantier introuvable."; return RedirectToAction(nameof(Index)); }

        var prev = new PrevisionGlobale
        {
            ChantierId = dto.ChantierId,
            DateCreation = dto.DateCreation,
            Reference = $"PGLOB-{chantier.Code}-{dto.DateCreation:yyyyMMdd}-{DateTime.UtcNow.Ticks % 10000:D4}",
            Statut = StatutPrevisionGlobale.Brouillon,
            Observation = dto.Observation,
            Lignes = dto.Lignes.Select(l => new PrevisionGlobaleLigne
            {
                Rubrique = l.Rubrique,
                Designation = l.Designation,
                Unite = l.Unite,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire,
                Observation = l.Observation
            }).ToList()
        };
        await _uow.PrevisionsGlobales.AddAsync(prev, ct);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Prévision globale enregistrée (brouillon).";
        return RedirectToAction(nameof(Details), new { id = prev.Id });
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var prev = await _uow.PrevisionsGlobales.Query().AsNoTracking()
            .Include(p => p.Lignes).FirstOrDefaultAsync(p => p.Id == id, ct);
        if (prev is null) return NotFound();
        if (!prev.EstModifiable)
        {
            TempData["Error"] = "Cette prévision n'est plus modifiable.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await ChargerChantiersAsync(ct);
        ViewBag.PrevId = id;
        var dto = new PrevisionGlobaleCreateDto
        {
            ChantierId = prev.ChantierId,
            DateCreation = prev.DateCreation,
            Observation = prev.Observation,
            Lignes = prev.Lignes.Select(l => new PrevisionGlobaleLigneCreateDto
            {
                Rubrique = l.Rubrique, Designation = l.Designation, Unite = l.Unite,
                Quantite = l.Quantite, PrixUnitaire = l.PrixUnitaire, Observation = l.Observation
            }).ToList()
        };
        return View("Create", dto);
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, PrevisionGlobaleCreateDto dto, CancellationToken ct)
    {
        var prev = await _uow.PrevisionsGlobales.Query()
            .Include(p => p.Lignes).FirstOrDefaultAsync(p => p.Id == id, ct);
        if (prev is null) return NotFound();
        if (!prev.EstModifiable)
        {
            TempData["Error"] = "Cette prévision n'est plus modifiable.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (dto.Lignes is null || dto.Lignes.Count == 0)
            ModelState.AddModelError(string.Empty, "Ajoutez au moins une ligne.");
        if (!ModelState.IsValid)
        {
            await ChargerChantiersAsync(ct);
            ViewBag.PrevId = id;
            return View("Create", dto);
        }

        prev.ChantierId = dto.ChantierId;
        prev.DateCreation = dto.DateCreation;
        prev.Observation = dto.Observation;

        foreach (var ancienne in prev.Lignes.ToList())
            _uow.PrevisionsGlobalesLignes.Remove(ancienne);
        prev.Lignes = dto.Lignes.Select(l => new PrevisionGlobaleLigne
        {
            Rubrique = l.Rubrique, Designation = l.Designation, Unite = l.Unite,
            Quantite = l.Quantite, PrixUnitaire = l.PrixUnitaire, Observation = l.Observation
        }).ToList();

        _uow.PrevisionsGlobales.Update(prev);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Prévision globale modifiée.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // --- Workflow : Chef soumet -> RF valide -> Admin valide -> Admin met en banque ---

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Soumettre(long id, CancellationToken ct)
    {
        var prev = await _uow.PrevisionsGlobales.GetByIdAsync(id, ct);
        if (prev is null) return NotFound();
        if (prev.Statut != StatutPrevisionGlobale.Brouillon && prev.Statut != StatutPrevisionGlobale.Refusee)
        {
            TempData["Error"] = "Seule une prévision en brouillon ou refusée peut être soumise.";
            return RedirectToAction(nameof(Details), new { id });
        }
        prev.Statut = StatutPrevisionGlobale.Soumise;
        prev.SoumisePar = User.Identity?.Name;
        prev.DateSoumission = DateTime.UtcNow;
        prev.MotifRefus = null;
        _uow.PrevisionsGlobales.Update(prev);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Prévision globale soumise pour validation.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValiderRf(long id, CancellationToken ct)
    {
        var prev = await _uow.PrevisionsGlobales.GetByIdAsync(id, ct);
        if (prev is null) return NotFound();
        if (prev.Statut != StatutPrevisionGlobale.Soumise)
        {
            TempData["Error"] = "Cette prévision n'est pas en attente de validation RF.";
            return RedirectToAction(nameof(Details), new { id });
        }
        prev.Statut = StatutPrevisionGlobale.ValideeResponsableFinancier;
        prev.ValideeParRfId = User.Identity?.Name;
        prev.DateValidationRf = DateTime.UtcNow;
        _uow.PrevisionsGlobales.Update(prev);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Prévision validée par le Responsable Financier. En attente de l'Administrateur.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValiderAdmin(long id, CancellationToken ct)
    {
        var prev = await _uow.PrevisionsGlobales.GetByIdAsync(id, ct);
        if (prev is null) return NotFound();
        if (prev.Statut != StatutPrevisionGlobale.ValideeResponsableFinancier)
        {
            TempData["Error"] = "Cette prévision doit d'abord être validée par le Responsable Financier.";
            return RedirectToAction(nameof(Details), new { id });
        }
        prev.Statut = StatutPrevisionGlobale.ValideeAdministrateur;
        prev.ValideeParAdminId = User.Identity?.Name;
        prev.DateValidationAdmin = DateTime.UtcNow;
        _uow.PrevisionsGlobales.Update(prev);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Prévision validée par l'Administrateur. Elle peut être mise en banque.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refuser(long id, string motif, CancellationToken ct)
    {
        var prev = await _uow.PrevisionsGlobales.GetByIdAsync(id, ct);
        if (prev is null) return NotFound();
        prev.Statut = StatutPrevisionGlobale.Refusee;
        prev.MotifRefus = motif;
        _uow.PrevisionsGlobales.Update(prev);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Prévision globale refusée.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Met le montant total en banque : dépôt sur le compte du chantier.</summary>
    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MettreEnBanque(long id, CancellationToken ct)
    {
        var prev = await _uow.PrevisionsGlobales.Query()
            .Include(p => p.Lignes).Include(p => p.Chantier)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (prev is null) return NotFound();
        if (prev.Statut != StatutPrevisionGlobale.ValideeAdministrateur)
        {
            TempData["Error"] = "La prévision doit être validée par l'Administrateur avant la mise en banque.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var compte = (await _uow.ComptesBancaires.ListAsync(
            c => c.ChantierId == prev.ChantierId && c.Type == TypeCompteBancaire.Chantier, ct)).FirstOrDefault();
        if (compte is null)
        {
            TempData["Error"] = "Aucun compte bancaire rattaché à ce chantier. Créez-le d'abord.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Le plan du projet est un PLAN, pas un retrait : rien ne sort d'ici. On exigeait
        // auparavant que le solde couvre la totalité du plan, parce que l'ancien code
        // déposait le marché entier à la création du chantier. Sur un marché public payé
        // par avance puis décomptes successifs, cette condition est intenable : elle
        // bloquerait l'activation du plan tant que l'intégralité n'est pas encaissée.
        //
        // La trésorerie est contrôlée là où l'argent sort réellement — à l'exécution
        // d'une prévision journalière (PrevisionService, « L'ARGENT SORT ICI »). Ici on
        // se contente d'avertir quand le compte ne couvre pas encore le plan.
        var total = prev.Total;

        prev.Statut = StatutPrevisionGlobale.MiseEnBanque;
        prev.DateMiseEnBanque = DateTime.UtcNow;
        _uow.PrevisionsGlobales.Update(prev);

        await _uow.SaveChangesAsync(ct);

        if (compte.Solde < total)
        {
            TempData["Success"] =
                $"Plan du projet activé. Budget de référence : {total:N0} Ar. " +
                $"À noter : le compte {compte.Nom} n'affiche que {compte.Solde:N0} Ar — " +
                $"il manque {(total - compte.Solde):N0} Ar, à encaisser au fil des décomptes. " +
                "Les prévisions journalières seront refusées si le compte est à sec le jour du retrait.";
        }
        else
        {
            TempData["Success"] = $"Plan du projet activé. Budget de référence : {total:N0} Ar, " +
                                  $"couvert par le compte {compte.Nom} ({compte.Solde:N0} Ar).";
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
