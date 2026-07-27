using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Interfaces;
using ETAM.Infrastructure.Identity;
using ETAM.Web.Models;
using ETAM.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

[Authorize]
public class MateriauxController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IAlerteService _alertes;
    private readonly IReferenceDataCache _referenceData;
    private readonly UserManager<ApplicationUser> _userManager;

    public MateriauxController(IUnitOfWork uow, IAlerteService alertes,
        IReferenceDataCache referenceData, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _alertes = alertes;
        _referenceData = referenceData;
        _userManager = userManager;
    }

    /// <summary>
    /// Chantier d'affectation de l'utilisateur connecté. Un magasinier affecté à un chantier
    /// ne voit que le stock de CE chantier. Null = tous les chantiers (admin, correspondant...).
    /// </summary>
    private async Task<long?> ChantierAffecteAsync()
    {
        if (User.IsInRole("Administrateur") || User.IsInRole("Correspondant")) return null;
        var user = await _userManager.GetUserAsync(User);
        return user?.ChantierId;
    }

    // Liste du stock (vue d'origine). Le magasinier est redirigé vers SA fiche.
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (User.IsInRole("Magasinier"))
            return RedirectToAction(nameof(Fiche));

        var chantierAffecte = await ChantierAffecteAsync();
        var q = _uow.Materiaux.Query().AsNoTracking().Include(m => m.Chantier).AsQueryable();
        if (chantierAffecte is > 0) q = q.Where(m => m.ChantierId == chantierAffecte);

        var materiaux = await q
            .OrderBy(m => m.Chantier!.Nom).ThenBy(m => m.Designation)
            .ToListAsync(ct);
        return View(materiaux);
    }

    // ---------------------------------------------------------------------
    //  VUE MAGASINIER — Fiche de gestion des matériaux (liste des mouvements)
    // ---------------------------------------------------------------------
    // Liste des matériaux (fiche par article). Clic sur « Détails » -> historique des mouvements.
    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier,Magasinier")]
    public async Task<IActionResult> Fiche(CancellationToken ct)
    {
        var chantierAffecte = await ChantierAffecteAsync();
        var q = _uow.Materiaux.Query().AsNoTracking().Include(m => m.Chantier).AsQueryable();
        if (chantierAffecte is > 0)
        {
            q = q.Where(m => m.ChantierId == chantierAffecte);
            ViewBag.ChantierAffecte = (await _uow.Chantiers.GetByIdAsync(chantierAffecte.Value, ct))?.Nom;
        }

        var materiaux = await q
            .OrderBy(m => m.Chantier!.Nom).ThenBy(m => m.Designation)
            .ToListAsync(ct);
        return View(materiaux);
    }

    // --- Exports PDF / Excel ---

    private static List<ColonneExport<Materiau>> ColonnesStock() => new()
    {
        new("Chantier",    m => m.Chantier?.Nom ?? ""),
        new("Désignation", m => m.Designation),
        new("Localité",    m => m.Localite ?? ""),
        new("Unité",       m => m.Unite),
        new("Besoin",      m => m.Besoin.ToString("N0"), true),
        new("Entrées",     m => m.QuantiteRecue.ToString("N0"), true),
        new("Sorties",     m => m.QuantiteUtilisee.ToString("N0"), true),
        new("Stock dispo.",m => m.StockDisponible.ToString("N0"), true),
        new("Seuil",       m => m.SeuilMinimal.ToString("N0"), true),
        new("État",        m => m.EstStockCritique ? "Critique" : m.EstStockFaible ? "Faible" : "OK")
    };

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier,Magasinier")]
    public async Task<IActionResult> ExportStock(string format, CancellationToken ct)
    {
        var chantierAffecte = await ChantierAffecteAsync();
        var q = _uow.Materiaux.Query().AsNoTracking().Include(m => m.Chantier).AsQueryable();
        if (chantierAffecte is > 0) q = q.Where(m => m.ChantierId == chantierAffecte);
        var data = await q.OrderBy(m => m.Chantier!.Nom).ThenBy(m => m.Designation).ToListAsync(ct);

        var cols = ColonnesStock();
        if (format == "excel")
            return File(ExportService.Excel("Stock", data, cols),
                ExportService.MimeExcel, ExportService.NomFichier("Stock", "xlsx"));

        var sousTitre = chantierAffecte is > 0
            ? $"Chantier : {data.FirstOrDefault()?.Chantier?.Nom}"
            : "Tous les chantiers";
        return File(ExportService.Pdf("Stock des matériaux", sousTitre, data, cols,
                $"{data.Count} article(s)", paysage: true),
            ExportService.MimePdf, ExportService.NomFichier("Stock", "pdf"));
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier,Magasinier")]
    public async Task<IActionResult> ExportFiche(long id, string format, CancellationToken ct)
    {
        var mat = await _uow.Materiaux.Query().AsNoTracking()
            .Include(m => m.Chantier).FirstOrDefaultAsync(m => m.Id == id, ct);
        if (mat is null) return NotFound();

        var chantierAffecte = await ChantierAffecteAsync();
        if (chantierAffecte is > 0 && mat.ChantierId != chantierAffecte) return Forbid();

        var mvts = await _uow.MouvementsMateriau.Query().AsNoTracking()
            .Where(mm => mm.MateriauxId == id).OrderBy(mm => mm.DateMouvement).ToListAsync(ct);

        var cols = new List<ColonneExport<MouvementMateriau>>
        {
            new("Date",             m => m.DateMouvement.ToString("dd/MM/yy")),
            new("Besoin/objectif",  _ => mat.Besoin.ToString("N0"), true),
            new("Entrée",           m => m.QuantiteEntree > 0 ? m.QuantiteEntree.ToString("N0") : "", true),
            new("Sortie",           m => m.QuantiteSortie > 0 ? m.QuantiteSortie.ToString("N0") : "", true),
            new("Solde sur besoin", m => m.SoldeSurBesoin.ToString("N0"), true),
            new("Solde en stock",   m => m.SoldeEnStock.ToString("N0"), true),
            new("Motif",            m => m.Motif ?? "")
        };

        var nom = $"Fiche_{mat.Designation}".Replace(' ', '_');
        if (format == "excel")
            return File(ExportService.Excel(mat.Designation, mvts, cols),
                ExportService.MimeExcel, ExportService.NomFichier(nom, "xlsx"));

        var sousTitre = $"Article : {mat.Designation} · Localité : {mat.Localite ?? mat.Chantier?.Nom} · " +
                        $"Unité : {mat.Unite} · Besoin : {mat.Besoin:N0}";
        return File(ExportService.Pdf("Fiche de gestion des matériaux", sousTitre, mvts, cols,
                $"Stock disponible : {mat.StockDisponible:N0} {mat.Unite}"),
            ExportService.MimePdf, ExportService.NomFichier(nom, "pdf"));
    }

    // Détails d'un matériau : sa fiche de mouvements (Date, Besoin, Entrée, Sortie, Solde, Motif).
    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier,Magasinier")]
    public async Task<IActionResult> DetailsFiche(long id, CancellationToken ct)
    {
        var materiau = await _uow.Materiaux.Query().AsNoTracking()
            .Include(m => m.Chantier)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
        if (materiau is null) return NotFound();

        // Un magasinier ne peut pas ouvrir la fiche d'un autre chantier.
        var chantierAffecte = await ChantierAffecteAsync();
        if (chantierAffecte is > 0 && materiau.ChantierId != chantierAffecte) return Forbid();

        ViewBag.Mouvements = await _uow.MouvementsMateriau.Query().AsNoTracking()
            .Where(mm => mm.MateriauxId == id)
            .OrderBy(mm => mm.DateMouvement)
            .ToListAsync(ct);

        return View(materiau);
    }

    // Formulaire d'insertion de plusieurs lignes sur une même fiche (comme sur la fiche manuscrite).
    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier,Magasinier")]
    [HttpGet]
    public async Task<IActionResult> NouveauMouvement(long? materiauxId, CancellationToken ct)
    {
        var chantierMat = await ChantierAffecteAsync();
        var qMat = _uow.Materiaux.Query().AsNoTracking().Include(m => m.Chantier).AsQueryable();
        if (chantierMat is > 0) qMat = qMat.Where(m => m.ChantierId == chantierMat);
        ViewBag.Materiaux = await qMat
            .OrderBy(m => m.Chantier!.Nom).ThenBy(m => m.Designation)
            .ToListAsync(ct);

        var vm = new SaisieFicheViewModel
        {
            MateriauxId = materiauxId ?? 0,
            Lignes = new List<LigneMouvementViewModel>
            {
                new() { DateMouvement = DateTime.UtcNow, QuantiteEntree = 0, QuantiteSortie = 0 }
            }
        };
        return View(vm);
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier,Magasinier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NouveauMouvement(SaisieFicheViewModel model, CancellationToken ct)
    {
        var materiau = await _uow.Materiaux.GetByIdAsync(model.MateriauxId, ct);
        if (materiau is null)
        {
            TempData["Error"] = "Article non trouvé.";
            return RedirectToAction(nameof(NouveauMouvement));
        }

        // On ne garde que les lignes ayant une entrée ou une sortie.
        var lignes = (model.Lignes ?? new())
            .Where(l => l.QuantiteEntree > 0 || l.QuantiteSortie > 0)
            .ToList();

        if (lignes.Count == 0)
        {
            TempData["Error"] = "Ajoutez au moins une ligne avec une entrée ou une sortie.";
            return RedirectToAction(nameof(NouveauMouvement), new { materiauxId = model.MateriauxId });
        }

        // Point de départ = état actuel de la fiche. S'il n'y a pas encore de mouvement,
        // on repart de l'état actuel de l'article (stock disponible et quantité reçue),
        // pour rester cohérent avec la liste « Stock » vue par les autres utilisateurs.
        var dernier = await _uow.MouvementsMateriau.Query().AsNoTracking()
            .Where(mm => mm.MateriauxId == model.MateriauxId)
            .OrderByDescending(mm => mm.DateMouvement).ThenByDescending(mm => mm.Id)
            .FirstOrDefaultAsync(ct);

        decimal soldeStock = dernier?.SoldeEnStock ?? materiau.StockDisponible;
        decimal totalEntrees = dernier is null
            ? materiau.QuantiteRecue
            : await _uow.MouvementsMateriau.Query().AsNoTracking()
                .Where(mm => mm.MateriauxId == model.MateriauxId)
                .SumAsync(mm => (decimal?)mm.QuantiteEntree, ct) ?? 0;

        int ordre = 0;
        foreach (var l in lignes)
        {
            soldeStock += l.QuantiteEntree - l.QuantiteSortie;
            if (soldeStock < 0)
            {
                TempData["Error"] = $"Stock insuffisant à la ligne {ordre + 1} : le solde deviendrait négatif.";
                return RedirectToAction(nameof(NouveauMouvement), new { materiauxId = model.MateriauxId });
            }
            totalEntrees += l.QuantiteEntree;

            await _uow.MouvementsMateriau.AddAsync(new MouvementMateriau
            {
                MateriauxId = model.MateriauxId,
                DateMouvement = (l.DateMouvement ?? DateTime.UtcNow),
                QuantiteEntree = l.QuantiteEntree,
                QuantiteSortie = l.QuantiteSortie,
                Motif = l.Motif,
                SoldeSurBesoin = materiau.Besoin - totalEntrees,
                SoldeEnStock = soldeStock
            }, ct);

            materiau.QuantiteRecue += l.QuantiteEntree;
            materiau.QuantiteUtilisee += l.QuantiteSortie;
            ordre++;
        }

        _uow.Materiaux.Update(materiau);
        await _uow.SaveChangesAsync(ct);
        await _alertes.EvaluerAlertesAsync(ct);

        TempData["Success"] = $"{lignes.Count} ligne(s) enregistrée(s) pour {materiau.Designation}.";
        return RedirectToAction(nameof(DetailsFiche), new { id = model.MateriauxId });
    }

    // ---------------------------------------------------------------------
    //  GESTION DES MATÉRIAUX (vue d'origine — hors magasinier)
    // ---------------------------------------------------------------------
    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
        return View(new Materiau());
    }

    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Materiau model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Chantiers = await _referenceData.ObtenirChantiersAsync(ct);
            return View(model);
        }
        await _uow.Materiaux.AddAsync(model, ct);
        await _uow.SaveChangesAsync(ct);
        TempData["Success"] = $"Matériau « {model.Designation} » ajouté.";
        return RedirectToAction(nameof(Index));
    }

    // Réception de matériel : augmente la quantité reçue + enregistre un mouvement.
    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Receptionner(long id, decimal quantite, CancellationToken ct)
    {
        var mat = await _uow.Materiaux.GetByIdAsync(id, ct);
        if (mat is null) return NotFound();

        mat.QuantiteRecue += quantite;
        _uow.Materiaux.Update(mat);
        await EnregistrerMouvementAsync(id, entree: quantite, sortie: 0, motif: "Réception", ct);
        await _uow.SaveChangesAsync(ct);
        await _alertes.EvaluerAlertesAsync(ct);
        TempData["Success"] = $"Réception de {quantite} {mat.Unite} enregistrée pour {mat.Designation}.";
        return RedirectToAction(nameof(Index));
    }

    // Sortie de stock : augmente la quantité utilisée + enregistre un mouvement.
    [Authorize(Roles = "Administrateur,Correspondant,Chef de chantier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sortir(long id, decimal quantite, CancellationToken ct)
    {
        var mat = await _uow.Materiaux.GetByIdAsync(id, ct);
        if (mat is null) return NotFound();

        if (quantite <= 0) { TempData["Error"] = "La quantité doit être positive."; return RedirectToAction(nameof(Index)); }
        if (quantite > mat.StockDisponible)
        {
            TempData["Error"] = $"Stock insuffisant : disponible {mat.StockDisponible:N2} {mat.Unite}.";
            return RedirectToAction(nameof(Index));
        }

        mat.QuantiteUtilisee += quantite;
        _uow.Materiaux.Update(mat);
        await EnregistrerMouvementAsync(id, entree: 0, sortie: quantite, motif: "Sortie de stock", ct);
        await _uow.SaveChangesAsync(ct);
        await _alertes.EvaluerAlertesAsync(ct);
        TempData["Success"] = $"Sortie de {quantite} {mat.Unite} enregistrée pour {mat.Designation}.";
        return RedirectToAction(nameof(Index));
    }

    // Ajoute une ligne dans la fiche de mouvements (sans SaveChanges — fait par l'appelant).
    private async Task EnregistrerMouvementAsync(long materiauxId, decimal entree, decimal sortie, string motif, CancellationToken ct)
    {
        var dernier = await _uow.MouvementsMateriau.Query().AsNoTracking()
            .Where(mm => mm.MateriauxId == materiauxId)
            .OrderByDescending(mm => mm.DateMouvement)
            .FirstOrDefaultAsync(ct);

        decimal solde = (dernier?.SoldeEnStock ?? 0) + entree - sortie;

        var mat = await _uow.Materiaux.Query().AsNoTracking().FirstOrDefaultAsync(m => m.Id == materiauxId, ct);
        decimal totalEntrees = await _uow.MouvementsMateriau.Query().AsNoTracking()
            .Where(mm => mm.MateriauxId == materiauxId)
            .SumAsync(mm => (decimal?)mm.QuantiteEntree, ct) ?? 0;
        decimal soldeSurBesoin = (mat?.Besoin ?? 0) - (totalEntrees + entree);

        await _uow.MouvementsMateriau.AddAsync(new MouvementMateriau
        {
            MateriauxId = materiauxId,
            DateMouvement = DateTime.UtcNow,
            QuantiteEntree = entree,
            QuantiteSortie = sortie,
            Motif = motif,
            SoldeSurBesoin = soldeSurBesoin,
            SoldeEnStock = solde
        }, ct);
    }
}
