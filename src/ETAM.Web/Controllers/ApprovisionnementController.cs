using ETAM.Application.DTOs;
using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using ETAM.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

[Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
public class ApprovisionnementController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IPrevisionService _prevision;
    private readonly IReferenceDataCache _referenceData;

    public ApprovisionnementController(IUnitOfWork uow, IPrevisionService prevision, IReferenceDataCache referenceData)
    {
        _uow = uow;
        _prevision = prevision;
        _referenceData = referenceData;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var appros = await _uow.Approvisionnements.Query().AsNoTracking()
            .Include(a => a.Chantier)
            .Include(a => a.Lignes)
            .OrderByDescending(a => a.DateAppro).Take(200).ToListAsync(ct);
        return View(appros);
    }

    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var appro = await _uow.Approvisionnements.Query().AsNoTracking()
            .Include(a => a.Chantier).Include(a => a.Lignes)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        if (appro is null) return NotFound();
        return View(appro);
    }

    /// <summary>Export PDF / Excel de la liste des approvisionnements.</summary>
    public async Task<IActionResult> ExportListe(string format, CancellationToken ct)
    {
        var data = await _uow.Approvisionnements.Query().AsNoTracking()
            .Include(a => a.Chantier).Include(a => a.Lignes)
            .OrderByDescending(a => a.DateAppro).Take(500).ToListAsync(ct);

        var cols = new List<ColonneExport<Approvisionnement>>
        {
            new("Référence", a => a.Reference),
            new("Chantier",  a => a.Chantier?.Nom ?? ""),
            new("Date",      a => a.DateAppro.ToString("dd/MM/yyyy")),
            new("Statut",    a => a.Statut.ToString()),
            new("Total (Ar)", a => a.Lignes.Sum(l => l.Quantite * l.PrixUnitaireEstime).ToString("N0"), true)
        };

        if (format == "excel")
            return File(ExportService.Excel("Approvisionnements", data, cols),
                ExportService.MimeExcel, ExportService.NomFichier("Approvisionnements", "xlsx"));

        var total = data.Sum(a => a.Lignes.Sum(l => l.Quantite * l.PrixUnitaireEstime));
        return File(ExportService.Pdf("Approvisionnements", "Demandes d'approvisionnement", data, cols,
                $"{data.Count} demande(s) · Total : {total:N0} Ar"),
            ExportService.MimePdf, ExportService.NomFichier("Approvisionnements", "pdf"));
    }

    /// <summary>Export PDF / Excel du détail d'un approvisionnement.</summary>
    public async Task<IActionResult> ExportDetail(long id, string format, CancellationToken ct)
    {
        var a = await _uow.Approvisionnements.Query().AsNoTracking()
            .Include(x => x.Chantier).Include(x => x.Lignes)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (a is null) return NotFound();

        var cols = new List<ColonneExport<ApprovisionnementLigne>>
        {
            new("Désignation", l => l.Designation),
            new("Catégorie",   l => l.Categorie),
            new("Budget",      l => l.TypeBudget.ToString()),
            new("Quantité",    l => l.Quantite.ToString("N2"), true),
            new("Prix unit.",  l => l.PrixUnitaireEstime.ToString("N0"), true),
            new("Total",       l => (l.Quantite * l.PrixUnitaireEstime).ToString("N0"), true)
        };

        var nom = $"Appro_{a.Reference}".Replace(' ', '_');
        if (format == "excel")
            return File(ExportService.Excel(a.Reference, a.Lignes, cols),
                ExportService.MimeExcel, ExportService.NomFichier(nom, "xlsx"));

        var sousTitre = $"{a.Chantier?.Nom} · {a.DateAppro:dd/MM/yyyy} · Statut : {a.Statut}";
        return File(ExportService.Pdf($"Approvisionnement {a.Reference}", sousTitre, a.Lignes, cols,
                $"Total : {a.Total:N0} Ar"),
            ExportService.MimePdf, ExportService.NomFichier(nom, "pdf"));
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        await ChargerListesAsync(ct);
        return View(new ApprovisionnementCreateDto());
    }

    /// <summary>
    /// Sécurité serveur : le prix d'un article du catalogue est imposé et ne peut pas
    /// être modifié depuis le formulaire (protection anti-fraude).
    /// </summary>
    private async Task AppliquerPrixCatalogueAsync(ApprovisionnementCreateDto dto, CancellationToken ct)
    {
        if (dto.Lignes is null || dto.Lignes.Count == 0) return;

        var catalogue = await _uow.Catalogue.Query().AsNoTracking()
            .ToDictionaryAsync(a => a.Designation.Trim().ToLower(), a => a.PrixUnitaire, ct);

        foreach (var l in dto.Lignes)
        {
            if (string.IsNullOrWhiteSpace(l.Designation)) continue;
            if (catalogue.TryGetValue(l.Designation.Trim().ToLower(), out var prixRef))
                l.PrixUnitaireEstime = prixRef;
        }
    }

    // Charge le catalogue (désignations + prix) pour l'autocomplétion et le pré-remplissage.
    private async Task ChargerListesAsync(CancellationToken ct)
    {
        ViewBag.Catalogue = (await _uow.Catalogue.ListAllAsync(ct))
            .OrderBy(a => a.Designation).ToList();

        var predefinies = new[] { "Nourriture", "Carburant", "Eau", "Transport", "Gros œuvre",
            "Ferraillage", "Forage", "Bois", "Consommable", "Location", "Déplacement", "Réparation" };
        var catCatalogue = (await _uow.Catalogue.Query().Select(a => a.Categorie).ToListAsync(ct));
        ViewBag.Categories = catCatalogue.Concat(predefinies)
            .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApprovisionnementCreateDto dto, CancellationToken ct)
    {
        if (dto.Lignes is null || dto.Lignes.Count == 0)
            ModelState.AddModelError(string.Empty, "Ajoutez au moins une ligne.");

        if (!ModelState.IsValid)
        {
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            await ChargerListesAsync(ct);
            return View(dto);
        }

        var chantier = await _uow.Chantiers.GetByIdAsync(dto.ChantierId, ct);
        if (chantier is null) { TempData["Error"] = "Chantier introuvable."; return RedirectToAction(nameof(Index)); }

        await AppliquerPrixCatalogueAsync(dto, ct);

        var appro = new Approvisionnement
        {
            ChantierId = dto.ChantierId,
            DateAppro = dto.DateAppro,
            Reference = $"APPRO-{chantier.Code}-{dto.DateAppro:yyyyMMdd}-{DateTime.UtcNow.Ticks % 10000:D4}",
            Statut = StatutApprovisionnement.Brouillon,
            Observation = dto.Observation,
            Lignes = dto.Lignes.Select(l => new ApprovisionnementLigne
            {
                Designation = l.Designation,
                Categorie = l.Categorie,
                TypeBudget = l.TypeBudget,
                MateriauId = l.MateriauId,
                DetteFournisseurId = l.DetteFournisseurId,
                Quantite = l.Quantite,
                PrixUnitaireEstime = l.PrixUnitaireEstime,
                Observation = l.Observation
            }).ToList()
        };
        await _uow.Approvisionnements.AddAsync(appro, ct);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Approvisionnement enregistré (brouillon).";
        return RedirectToAction(nameof(Index));
    }

    // --- Modification d'un approvisionnement encore en Brouillon ---
    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var appro = await _uow.Approvisionnements.Query().AsNoTracking()
            .Include(a => a.Lignes).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (appro is null) return NotFound();
        if (appro.Statut != StatutApprovisionnement.Brouillon)
        {
            TempData["Error"] = "Cet approvisionnement est validé : il n'est plus modifiable.";
            return RedirectToAction(nameof(Details), new { id });
        }

        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        await ChargerListesAsync(ct);
        var dto = new ApprovisionnementCreateDto
        {
            ChantierId = appro.ChantierId,
            DateAppro = appro.DateAppro,
            Observation = appro.Observation,
            Lignes = appro.Lignes.Select(l => new ApprovisionnementLigneCreateDto
            {
                Designation = l.Designation, Categorie = l.Categorie, TypeBudget = l.TypeBudget,
                Quantite = l.Quantite, PrixUnitaireEstime = l.PrixUnitaireEstime, Observation = l.Observation
            }).ToList()
        };
        ViewBag.ApproId = id;
        return View(dto);
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, ApprovisionnementCreateDto dto, CancellationToken ct)
    {
        var appro = await _uow.Approvisionnements.Query()
            .Include(a => a.Lignes).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (appro is null) return NotFound();
        if (appro.Statut != StatutApprovisionnement.Brouillon)
        {
            TempData["Error"] = "Cet approvisionnement est validé : il n'est plus modifiable.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (dto.Lignes is null || dto.Lignes.Count == 0)
            ModelState.AddModelError(string.Empty, "Ajoutez au moins une ligne.");
        if (!ModelState.IsValid)
        {
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            await ChargerListesAsync(ct);
            ViewBag.ApproId = id;
            return View(dto);
        }

        await AppliquerPrixCatalogueAsync(dto, ct);

        appro.ChantierId = dto.ChantierId;
        appro.DateAppro = dto.DateAppro;
        appro.Observation = dto.Observation;

        // Remplace les lignes.
        foreach (var ancienne in appro.Lignes.ToList())
            _uow.ApprovisionnementLignes.Remove(ancienne);
        appro.Lignes = dto.Lignes.Select(l => new ApprovisionnementLigne
        {
            Designation = l.Designation, Categorie = l.Categorie, TypeBudget = l.TypeBudget,
            Quantite = l.Quantite, PrixUnitaireEstime = l.PrixUnitaireEstime, Observation = l.Observation
        }).ToList();

        _uow.Approvisionnements.Update(appro);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Approvisionnement modifié.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Valide l'approvisionnement (rôle Responsable) : génère une Prévision journalière
    /// soumise à partir de ses lignes, puis marque l'appro comme Validé.
    /// </summary>
    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Convertir(long id, CancellationToken ct)
    {
        var appro = await _uow.Approvisionnements.Query()
            .Include(a => a.Lignes)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        if (appro is null) return NotFound();
        if (appro.Statut != StatutApprovisionnement.Brouillon)
        {
            TempData["Error"] = "Cet approvisionnement a déjà été validé ou annulé.";
            return RedirectToAction(nameof(Index));
        }

        // Blocage propre à CE chantier : les travaux de la prévision précédente
        // doivent être justifiés et réceptionnés avant d'en générer une nouvelle.
        var bloquante = await _uow.Previsions.Query().AsNoTracking()
            .Where(p => p.ChantierId == appro.ChantierId &&
                        (p.Statut == StatutPrevision.Executee || p.Statut == StatutPrevision.RapportSoumis))
            .OrderByDescending(p => p.DatePrevision)
            .FirstOrDefaultAsync(ct);
        if (bloquante is not null)
        {
            TempData["Error"] = bloquante.Statut == StatutPrevision.Executee
                ? $"Chantier bloqué : la prévision {bloquante.Reference} attend le compte rendu des travaux."
                : $"Chantier bloqué : le compte rendu de {bloquante.Reference} attend la réception de l'Administrateur.";
            return RedirectToAction("Details", "Prevision", new { id = bloquante.Id });
        }

        var dto = new PrevisionCreateDto
        {
            ChantierId = appro.ChantierId,
            DatePrevision = appro.DateAppro,
            Observation = $"Généré depuis l'approvisionnement {appro.Reference}." +
                          (string.IsNullOrEmpty(appro.Observation) ? "" : $" {appro.Observation}"),
            Lignes = appro.Lignes.Select(l => new PrevisionLigneCreateDto
            {
                Designation = l.Designation,
                Categorie = l.Categorie,
                TypeBudget = l.TypeBudget,
                MateriauId = l.MateriauId,
                DetteFournisseurId = l.DetteFournisseurId,
                Quantite = l.Quantite,
                PrixUnitaireEstime = l.PrixUnitaireEstime,
                Observation = l.Observation
            }).ToList()
        };

        var creation = await _prevision.CreerAsync(dto, ct);
        if (!creation.Succeeded)
        {
            TempData["Error"] = creation.Error;
            return RedirectToAction(nameof(Index));
        }

        var previsionId = creation.Data;

        // Soumet directement la prévision générée (entre dans le workflow de validation).
        await _prevision.SoumettreAsync(previsionId, ct);

        appro.Statut = StatutApprovisionnement.Valide;
        appro.PrevisionJournaliereId = previsionId;
        _uow.Approvisionnements.Update(appro);
        await _uow.SaveChangesAsync(ct);

        TempData["Success"] = "Approvisionnement validé et converti en prévision (soumise pour validation).";
        return RedirectToAction("Index", "Prevision");
    }
}
