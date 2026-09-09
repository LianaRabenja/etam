using ETAM.Application.DTOs;
using ETAM.Application.Interfaces;
using ETAM.Domain.Enums;
using ETAM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Infrastructure.Services;

/// <summary>Calcule les KPI et séries du tableau de bord.</summary>
public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context) => _context = context;

    public async Task<DashboardDto> ObtenirAsync(CancellationToken ct = default)
    {
        var dto = new DashboardDto();
        var maintenant = DateTime.UtcNow;
        var debutMois = new DateTime(maintenant.Year, maintenant.Month, 1);

        var budget = await _context.BudgetsComptes
            .Where(b => b.EstActif).OrderByDescending(b => b.Annee).FirstOrDefaultAsync(ct);
        if (budget is not null)
        {
            dto.BudgetCompteRestant = budget.MontantRestant;
            dto.BudgetCompteConsommePct = Math.Round(budget.PourcentageConsomme, 1);
        }

        var chantiers = await _context.Chantiers.ToListAsync(ct);
        dto.NombreChantiers = chantiers.Count;
        dto.BudgetMaterielRestant = chantiers.Sum(c => c.BudgetMaterielRestant);
        var budgetMatTotal = chantiers.Sum(c => c.BudgetMateriel);
        var consoMatTotal = chantiers.Sum(c => c.Consommation);
        dto.BudgetMaterielConsommePct = budgetMatTotal <= 0 ? 0
            : Math.Round((double)(consoMatTotal / budgetMatTotal) * 100d, 1);

        dto.DepensesDuMois = await _context.Depenses
            .Where(d => d.Date >= debutMois).SumAsync(d => (decimal?)(d.Quantite * d.PrixUnitaire), ct) ?? 0m;

        var materiaux = await _context.Materiaux.ToListAsync(ct);
        dto.ValeurTotaleMateriaux = materiaux.Sum(m => m.ValeurRestante);

        dto.NombreAlertes = await _context.Alertes.CountAsync(a => !a.EstLue, ct);
        dto.PrevisionsDuJour = await _context.Previsions
            .CountAsync(p => p.DatePrevision.Date == maintenant.Date, ct);
        dto.ValidationsEnAttente = await _context.Previsions
            .CountAsync(p => p.Statut == StatutPrevision.Soumise
                          || p.Statut == StatutPrevision.ValideeResponsableFinancier, ct);

        // Trésorerie : total des soldes bancaires et total restant dû aux fournisseurs.
        dto.TresorerieTotale = await _context.ComptesBancaires.SumAsync(c => (decimal?)c.Solde, ct) ?? 0m;
        var dettes = await _context.DettesFournisseurs.ToListAsync(ct);
        dto.TotalDettes = dettes.Where(d => d.Statut != StatutDette.Soldee).Sum(d => d.SoldeRestant);

        // Série : dépenses des 6 derniers mois.
        for (int i = 5; i >= 0; i--)
        {
            var mois = debutMois.AddMonths(-i);
            var moisSuivant = mois.AddMonths(1);
            var total = await _context.Depenses
                .Where(d => d.Date >= mois && d.Date < moisSuivant)
                .SumAsync(d => (decimal?)(d.Quantite * d.PrixUnitaire), ct) ?? 0m;
            dto.MoisLabels.Add(mois.ToString("MMM yy"));
            dto.DepensesMensuelles.Add(total);
        }

        // Consommation matériel par chantier.
        foreach (var c in chantiers.OrderByDescending(c => c.Consommation).Take(8))
        {
            dto.ChantiersLabels.Add(c.Nom);
            dto.ConsommationParChantier.Add(c.Consommation);
        }

        // Top matériaux consommés (par valeur utilisée).
        foreach (var m in materiaux
                     .OrderByDescending(m => m.QuantiteUtilisee * m.PrixUnitaire).Take(8))
        {
            dto.TopMateriauxLabels.Add(m.Designation);
            dto.TopMateriauxValeurs.Add(m.QuantiteUtilisee * m.PrixUnitaire);
        }

        return dto;
    }
}
