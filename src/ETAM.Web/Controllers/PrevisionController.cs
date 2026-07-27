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

[Authorize]
public class PrevisionController : Controller
{
    private readonly IPrevisionService _service;
    private readonly IUnitOfWork _uow;
    private readonly IReferenceDataCache _referenceData;

    public PrevisionController(IPrevisionService service, IUnitOfWork uow, IReferenceDataCache referenceData)
    {
        _service = service;
        _uow = uow;
        _referenceData = referenceData;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var previsions = await _uow.Previsions.Query().AsNoTracking()
            .Include(p => p.Chantier)
            .Include(p => p.Lignes)
            .OrderByDescending(p => p.DatePrevision)
            .Take(200)
            .ToListAsync(ct);
        return View(previsions);
    }

    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var prevision = await _uow.Previsions.Query().AsNoTracking()
            .Include(p => p.Chantier)
            .Include(p => p.Lignes).ThenInclude(l => l.PrevisionGlobaleLigne)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (prevision is null) return NotFound();

        // Contexte budgétaire pour aider le valideur à décider.
        var budget = (await _uow.BudgetsComptes.ListAsync(b => b.EstActif, ct))
            .OrderByDescending(b => b.Annee).FirstOrDefault();
        ViewBag.BudgetCompteRestant = budget?.MontantRestant ?? 0m;
        ViewBag.BudgetMaterielRestant = prevision.Chantier?.BudgetMaterielRestant ?? 0m;
        return View(prevision);
    }

    // Seuls l'Administrateur et le Correspondant créent une prévision (GET et POST alignés).
    // Le chef de chantier passe par un approvisionnement, qui devient une prévision à sa validation.
    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        await ChargerDettesAsync(ct);
        await ChargerCatalogueAsync(ct);
        await ChargerBlocagesAsync(ct);
        await ChargerPostesGlobauxAsync(ct);
        return View(new PrevisionCreateDto());
    }

    /// <summary>
    /// Charge les postes des prévisions globales actives (toutes chantiers confondus),
    /// avec l'enveloppe prévue, le montant déjà consommé et le reste. La vue filtre
    /// ensuite selon le chantier choisi.
    /// </summary>
    private async Task ChargerPostesGlobauxAsync(CancellationToken ct)
    {
        var lignes = await _uow.PrevisionsGlobalesLignes.Query().AsNoTracking()
            .Include(l => l.PrevisionGlobale)
            .Where(l => l.PrevisionGlobale.Statut == StatutPrevisionGlobale.ValideeAdministrateur
                     || l.PrevisionGlobale.Statut == StatutPrevisionGlobale.MiseEnBanque)
            .ToListAsync(ct);

        var ids = lignes.Select(l => l.Id).ToList();

        // Consommé = somme des lignes de prévisions engagées, rattachées à ce poste.
        var consomme = ids.Count == 0
            ? new Dictionary<long, decimal>()
            : await _uow.PrevisionLignes.Query().AsNoTracking()
                .Where(pl => pl.PrevisionGlobaleLigneId != null
                          && ids.Contains(pl.PrevisionGlobaleLigneId.Value)
                          && (pl.PrevisionJournaliere.Statut == StatutPrevision.ValideeAdministrateur
                           || pl.PrevisionJournaliere.Statut == StatutPrevision.Executee
                           || pl.PrevisionJournaliere.Statut == StatutPrevision.RapportSoumis
                           || pl.PrevisionJournaliere.Statut == StatutPrevision.Cloturee))
                .GroupBy(pl => pl.PrevisionGlobaleLigneId!.Value)
                .Select(g => new { Id = g.Key, Total = g.Sum(x => x.Quantite * x.PrixUnitaireEstime) })
                .ToDictionaryAsync(x => x.Id, x => x.Total, ct);

        ViewBag.PostesGlobaux = lignes
            .Select(l => new ETAM.Application.DTOs.PosteGlobalDto
            {
                Id = l.Id,
                ChantierId = l.PrevisionGlobale.ChantierId,
                Rubrique = l.Rubrique,
                Designation = l.Designation,
                Enveloppe = l.Quantite * l.PrixUnitaire,
                Consomme = consomme.TryGetValue(l.Id, out var c) ? c : 0m
            })
            .OrderBy(p => p.Rubrique).ThenBy(p => p.Designation)
            .ToList();
    }

    /// <summary>
    /// Détermine, CHANTIER PAR CHANTIER, ceux qui sont bloqués : une prévision déjà exécutée
    /// dont les travaux ne sont pas encore réceptionnés. Un chantier à jour reste libre
    /// même si un autre chantier est bloqué.
    /// </summary>
    private async Task ChargerBlocagesAsync(CancellationToken ct)
    {
        var enAttente = await _uow.Previsions.Query().AsNoTracking()
            .Where(p => p.Statut == StatutPrevision.Executee || p.Statut == StatutPrevision.RapportSoumis)
            .Select(p => new { p.ChantierId, p.Reference, p.Statut, p.DatePrevision })
            .ToListAsync(ct);

        // Une seule entrée par chantier (la plus récente).
        ViewBag.Blocages = enAttente
            .GroupBy(x => x.ChantierId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var d = g.OrderByDescending(x => x.DatePrevision).First();
                    return d.Statut == StatutPrevision.Executee
                        ? $"{d.Reference} : compte rendu des travaux non rendu"
                        : $"{d.Reference} : compte rendu en attente de réception par l'Administrateur";
                });
    }

    // Dettes non soldées, pour les lignes de remboursement de dette.
    private async Task ChargerDettesAsync(CancellationToken ct)
    {
        ViewBag.Dettes = await _uow.DettesFournisseurs.Query()
            .Include(d => d.Fournisseur)
            .Where(d => d.Statut != ETAM.Domain.Enums.StatutDette.Soldee)
            .ToListAsync(ct);
    }

    // Charge le catalogue (désignations + prix) pour l'autocomplétion et le pré-remplissage du prix.
    private async Task ChargerCatalogueAsync(CancellationToken ct)
    {
        ViewBag.Catalogue = (await _uow.Catalogue.ListAllAsync(ct))
            .OrderBy(a => a.Designation).ToList();

        var predefinies = new[] { "Nourriture", "Carburant", "Eau", "Transport", "Gros œuvre",
            "Ferraillage", "Forage", "Bois", "Consommable", "Location", "Déplacement", "Réparation" };
        var catCatalogue = await _uow.Catalogue.Query().Select(a => a.Categorie).ToListAsync(ct);
        ViewBag.Categories = catCatalogue.Concat(predefinies)
            .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PrevisionCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            await ChargerDettesAsync(ct);
            await ChargerCatalogueAsync(ct);
            await ChargerBlocagesAsync(ct);
            await ChargerPostesGlobauxAsync(ct);
            return View(dto);
        }
        await AppliquerPrixCatalogueAsync(dto, ct);

        // BLOCAGE MÉTIER : impossible de demander une nouvelle prévision tant que les travaux
        // financés par la précédente n'ont pas été réceptionnés par l'Administrateur.
        var bloquante = await TrouverPrevisionBloquanteAsync(dto.ChantierId, ct);
        if (bloquante is not null)
        {
            TempData["Error"] = bloquante.Statut == StatutPrevision.Executee
                ? $"Impossible : la prévision {bloquante.Reference} attend le compte rendu des travaux réalisés."
                : $"Impossible : le compte rendu de la prévision {bloquante.Reference} attend la réception de l'Administrateur.";
            return RedirectToAction(nameof(Details), new { id = bloquante.Id });
        }

        var result = await _service.CreerAsync(dto, ct);
        if (!result.Succeeded) { TempData["Error"] = result.Error; }
        else TempData["Success"] = "Prévision créée en brouillon.";
        return RedirectToAction(nameof(Index));
    }

    // --- Exports PDF / Excel ---

    /// <summary>Export de la liste des prévisions.</summary>
    public async Task<IActionResult> ExportListe(string format, CancellationToken ct)
    {
        var data = await _uow.Previsions.Query().AsNoTracking()
            .Include(p => p.Chantier).Include(p => p.Lignes)
            .OrderByDescending(p => p.DatePrevision).Take(500).ToListAsync(ct);

        var cols = new List<ColonneExport<PrevisionJournaliere>>
        {
            new("Référence", p => p.Reference),
            new("Chantier",  p => p.Chantier?.Nom ?? ""),
            new("Date",      p => p.DatePrevision.ToString("dd/MM/yyyy")),
            new("Statut",    p => p.Statut.ToString()),
            new("Montant (Ar)", p => p.Lignes.Sum(l => l.Quantite * l.PrixUnitaireEstime).ToString("N0"), true)
        };

        if (format == "excel")
            return File(ExportService.Excel("Previsions", data, cols),
                ExportService.MimeExcel, ExportService.NomFichier("Previsions", "xlsx"));

        var total = data.Sum(p => p.Lignes.Sum(l => l.Quantite * l.PrixUnitaireEstime));
        return File(ExportService.Pdf("Prévisions journalières", "Toutes les prévisions", data, cols,
                $"{data.Count} prévision(s) · Total : {total:N0} Ar"),
            ExportService.MimePdf, ExportService.NomFichier("Previsions", "pdf"));
    }

    /// <summary>Export du détail d'une prévision (ses lignes).</summary>
    public async Task<IActionResult> ExportDetail(long id, string format, CancellationToken ct)
    {
        var p = await _uow.Previsions.Query().AsNoTracking()
            .Include(x => x.Chantier).Include(x => x.Lignes)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();

        var cols = new List<ColonneExport<PrevisionLigne>>
        {
            new("Désignation", l => l.Designation),
            new("Catégorie",   l => l.Categorie),
            new("Budget",      l => l.TypeBudget.ToString()),
            new("Quantité",    l => l.Quantite.ToString("N2"), true),
            new("Prix unit.",  l => l.PrixUnitaireEstime.ToString("N0"), true),
            new("Total",       l => (l.Quantite * l.PrixUnitaireEstime).ToString("N0"), true)
        };

        var nom = $"Prevision_{p.Reference}".Replace(' ', '_');
        if (format == "excel")
            return File(ExportService.Excel(p.Reference, p.Lignes, cols),
                ExportService.MimeExcel, ExportService.NomFichier(nom, "xlsx"));

        var sousTitre = $"{p.Chantier?.Nom} · {p.DatePrevision:dd/MM/yyyy} · Statut : {p.Statut}";
        return File(ExportService.Pdf($"Prévision {p.Reference}", sousTitre, p.Lignes, cols,
                $"Total : {p.Total:N0} Ar"),
            ExportService.MimePdf, ExportService.NomFichier(nom, "pdf"));
    }

    /// <summary>
    /// Sécurité serveur : si une désignation existe au catalogue, son prix de référence
    /// est imposé — impossible de le modifier depuis le formulaire (protection anti-fraude).
    /// </summary>
    private async Task AppliquerPrixCatalogueAsync(PrevisionCreateDto dto, CancellationToken ct)
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

    /// <summary>Prévision exécutée d'un chantier dont les travaux ne sont pas encore réceptionnés.</summary>
    private async Task<PrevisionJournaliere?> TrouverPrevisionBloquanteAsync(long chantierId, CancellationToken ct)
        => await _uow.Previsions.Query().AsNoTracking()
            .Where(p => p.ChantierId == chantierId &&
                        (p.Statut == StatutPrevision.Executee || p.Statut == StatutPrevision.RapportSoumis))
            .OrderByDescending(p => p.DatePrevision)
            .FirstOrDefaultAsync(ct);

    // --- Compte rendu des travaux (chef) puis réception par l'Administrateur ---

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SoumettreRapport(long id, string rapport, CancellationToken ct)
    {
        var prev = await _uow.Previsions.GetByIdAsync(id, ct);
        if (prev is null) return NotFound();
        if (prev.Statut != StatutPrevision.Executee)
        {
            TempData["Error"] = "Seule une prévision exécutée peut faire l'objet d'un compte rendu.";
            return RedirectToAction(nameof(Details), new { id });
        }
        if (string.IsNullOrWhiteSpace(rapport))
        {
            TempData["Error"] = "Décrivez les travaux réalisés.";
            return RedirectToAction(nameof(Details), new { id });
        }

        prev.RapportRealisation = rapport;
        prev.DateRapport = DateTime.UtcNow;
        prev.Statut = StatutPrevision.RapportSoumis;
        prev.MotifRefusRapport = null;
        _uow.Previsions.Update(prev);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Compte rendu envoyé à l'Administrateur pour réception.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReceptionnerTravaux(long id, CancellationToken ct)
    {
        var prev = await _uow.Previsions.GetByIdAsync(id, ct);
        if (prev is null) return NotFound();
        if (prev.Statut != StatutPrevision.RapportSoumis)
        {
            TempData["Error"] = "Aucun compte rendu en attente de réception.";
            return RedirectToAction(nameof(Details), new { id });
        }

        prev.Statut = StatutPrevision.Cloturee;
        prev.RapportValideParId = User.Identity?.Name;
        prev.DateValidationRapport = DateTime.UtcNow;
        _uow.Previsions.Update(prev);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Travaux réceptionnés. Une nouvelle prévision peut être demandée pour ce chantier.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefuserRapport(long id, string motif, CancellationToken ct)
    {
        var prev = await _uow.Previsions.GetByIdAsync(id, ct);
        if (prev is null) return NotFound();
        if (prev.Statut != StatutPrevision.RapportSoumis)
        {
            TempData["Error"] = "Aucun compte rendu en attente.";
            return RedirectToAction(nameof(Details), new { id });
        }

        prev.Statut = StatutPrevision.Executee; // renvoyé au chef pour correction
        prev.MotifRefusRapport = motif;
        _uow.Previsions.Update(prev);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Compte rendu renvoyé au chef de chantier.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // --- Modification d'une prévision non encore validée par l'Administrateur ---
    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var p = await _uow.Previsions.Query().AsNoTracking()
            .Include(x => x.Lignes).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();
        if (!p.EstModifiable)
        {
            TempData["Error"] = "Cette prévision est validée par l'Administrateur : elle n'est plus modifiable.";
            return RedirectToAction(nameof(Details), new { id });
        }

        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        await ChargerDettesAsync(ct);
        await ChargerCatalogueAsync(ct);
        await ChargerPostesGlobauxAsync(ct);
        ViewBag.PrevId = id;
        var dto = new PrevisionCreateDto
        {
            ChantierId = p.ChantierId,
            DatePrevision = p.DatePrevision,
            Observation = p.Observation,
            Lignes = p.Lignes.Select(l => new PrevisionLigneCreateDto
            {
                Designation = l.Designation, Categorie = l.Categorie, TypeBudget = l.TypeBudget,
                MateriauId = l.MateriauId, DetteFournisseurId = l.DetteFournisseurId,
                PrevisionGlobaleLigneId = l.PrevisionGlobaleLigneId,
                Quantite = l.Quantite, PrixUnitaireEstime = l.PrixUnitaireEstime, Observation = l.Observation
            }).ToList()
        };
        return View(dto);
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, PrevisionCreateDto dto, CancellationToken ct)
    {
        var p = await _uow.Previsions.Query()
            .Include(x => x.Lignes).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();
        if (!p.EstModifiable)
        {
            TempData["Error"] = "Cette prévision n'est plus modifiable.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (dto.Lignes is null || dto.Lignes.Count == 0)
            ModelState.AddModelError(string.Empty, "Ajoutez au moins une ligne.");
        if (!ModelState.IsValid)
        {
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            await ChargerDettesAsync(ct);
            await ChargerCatalogueAsync(ct);
            await ChargerPostesGlobauxAsync(ct);
            ViewBag.PrevId = id;
            return View(dto);
        }

        await AppliquerPrixCatalogueAsync(dto, ct);

        p.ChantierId = dto.ChantierId;
        p.DatePrevision = dto.DatePrevision;
        p.Observation = dto.Observation;

        foreach (var ancienne in p.Lignes.ToList())
            _uow.PrevisionLignes.Remove(ancienne);
        p.Lignes = dto.Lignes.Select(l => new ETAM.Domain.Entities.PrevisionLigne
        {
            Designation = l.Designation, Categorie = l.Categorie, TypeBudget = l.TypeBudget,
            MateriauId = l.MateriauId, DetteFournisseurId = l.DetteFournisseurId,
            PrevisionGlobaleLigneId = l.PrevisionGlobaleLigneId,
            Quantite = l.Quantite, PrixUnitaireEstime = l.PrixUnitaireEstime, Observation = l.Observation
        }).ToList();

        _uow.Previsions.Update(p);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = "Prévision modifiée.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Soumettre(long id, CancellationToken ct)
        => Retour(await _service.SoumettreAsync(id, ct));

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValiderRf(long id, CancellationToken ct)
        => Retour(await _service.ValiderResponsableFinancierAsync(id, ct));

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValiderAdmin(long id, CancellationToken ct)
        => Retour(await _service.ValiderAdministrateurAsync(id, ct));

    [Authorize(Roles = "Administrateur")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Executer(long id, bool utiliserReserve, CancellationToken ct)
        => Retour(await _service.ExecuterAsync(id, utiliserReserve, ct));

    [Authorize(Roles = "Administrateur,Correspondant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refuser(long id, string motif, CancellationToken ct)
        => Retour(await _service.RefuserAsync(id, motif ?? "Non précisé", ct));

    private IActionResult Retour(ETAM.Application.Common.Models.Result r)
    {
        if (r.Succeeded) TempData["Success"] = "Opération effectuée.";
        else TempData["Error"] = r.Error;
        return RedirectToAction(nameof(Index));
    }
}
