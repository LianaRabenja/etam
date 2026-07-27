using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Domain.Interfaces;
using ETAM.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Web.Controllers;

/// <summary>
/// Rapports de prévision : ce qui a réellement été fait avec l'argent de chaque prévision.
/// Permet à l'Administrateur de suivre la justification des dépenses, chantier par chantier.
/// </summary>
[Authorize]
public class RapportsPrevisionController : Controller
{
    private readonly IUnitOfWork _uow;

    public RapportsPrevisionController(IUnitOfWork uow) => _uow = uow;

    public async Task<IActionResult> Index(long? chantierId, string? filtre, CancellationToken ct)
    {
        var query = _uow.Previsions.Query().AsNoTracking()
            .Include(p => p.Chantier).Include(p => p.Lignes)
            // Seules les prévisions dont l'argent est sorti ont un compte rendu à fournir.
            .Where(p => p.Statut == StatutPrevision.Executee
                     || p.Statut == StatutPrevision.RapportSoumis
                     || p.Statut == StatutPrevision.Cloturee);

        if (chantierId is > 0)
            query = query.Where(p => p.ChantierId == chantierId);

        query = filtre switch
        {
            "attente"   => query.Where(p => p.Statut == StatutPrevision.Executee),
            "reception" => query.Where(p => p.Statut == StatutPrevision.RapportSoumis),
            "clos"      => query.Where(p => p.Statut == StatutPrevision.Cloturee),
            _           => query
        };

        var previsions = await query
            .OrderByDescending(p => p.DatePrevision).ThenByDescending(p => p.Id)
            .Take(300).ToListAsync(ct);

        ViewBag.Chantiers = await _uow.Chantiers.Query().AsNoTracking()
            .OrderBy(c => c.Nom).ToListAsync(ct);
        ViewBag.ChantierId = chantierId;
        ViewBag.Filtre = filtre;

        // Compteurs pour les onglets de filtre.
        var toutes = await _uow.Previsions.Query().AsNoTracking()
            .Where(p => p.Statut == StatutPrevision.Executee
                     || p.Statut == StatutPrevision.RapportSoumis
                     || p.Statut == StatutPrevision.Cloturee)
            .Select(p => p.Statut).ToListAsync(ct);
        ViewBag.NbAttente = toutes.Count(s => s == StatutPrevision.Executee);
        ViewBag.NbReception = toutes.Count(s => s == StatutPrevision.RapportSoumis);
        ViewBag.NbClos = toutes.Count(s => s == StatutPrevision.Cloturee);
        ViewBag.NbTotal = toutes.Count;

        return View(previsions);
    }

    /// <summary>Export PDF / Excel des rapports de prévision (avec les mêmes filtres).</summary>
    public async Task<IActionResult> Export(long? chantierId, string? filtre, string format, CancellationToken ct)
    {
        var query = _uow.Previsions.Query().AsNoTracking()
            .Include(p => p.Chantier).Include(p => p.Lignes)
            .Where(p => p.Statut == StatutPrevision.Executee
                     || p.Statut == StatutPrevision.RapportSoumis
                     || p.Statut == StatutPrevision.Cloturee);

        if (chantierId is > 0) query = query.Where(p => p.ChantierId == chantierId);
        query = filtre switch
        {
            "attente"   => query.Where(p => p.Statut == StatutPrevision.Executee),
            "reception" => query.Where(p => p.Statut == StatutPrevision.RapportSoumis),
            "clos"      => query.Where(p => p.Statut == StatutPrevision.Cloturee),
            _           => query
        };

        var data = await query.OrderByDescending(p => p.DatePrevision).Take(500).ToListAsync(ct);

        var cols = new List<ColonneExport<PrevisionJournaliere>>
        {
            new("Référence", p => p.Reference),
            new("Chantier",  p => p.Chantier?.Nom ?? ""),
            new("Date",      p => p.DatePrevision.ToString("dd/MM/yyyy")),
            new("Montant (Ar)", p => p.Lignes.Sum(l => l.Quantite * l.PrixUnitaireEstime).ToString("N0"), true),
            new("État", p => p.Statut switch
            {
                StatutPrevision.Executee      => "À justifier",
                StatutPrevision.RapportSoumis => "À réceptionner",
                StatutPrevision.Cloturee      => "Réceptionné",
                _                             => p.Statut.ToString()
            }),
            new("Travaux réalisés", p => string.IsNullOrWhiteSpace(p.RapportRealisation)
                ? "— AUCUN COMPTE RENDU —" : p.RapportRealisation),
            new("Réceptionné par", p => p.RapportValideParId ?? "")
        };

        if (format == "excel")
            return File(ExportService.Excel("Rapports prevision", data, cols),
                ExportService.MimeExcel, ExportService.NomFichier("RapportsPrevision", "xlsx"));

        var nonJustifies = data.Count(p => string.IsNullOrWhiteSpace(p.RapportRealisation));
        return File(ExportService.Pdf("Rapports de prévision",
                "Justification des dépenses par prévision", data, cols,
                $"{data.Count} prévision(s) · {nonJustifies} sans compte rendu", paysage: true),
            ExportService.MimePdf, ExportService.NomFichier("RapportsPrevision", "pdf"));
    }
}
