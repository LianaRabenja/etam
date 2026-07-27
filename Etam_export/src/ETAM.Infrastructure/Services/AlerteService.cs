using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ETAM.Infrastructure.Services;

/// <summary>Génération et évaluation automatique des alertes (budget, stock, validation).</summary>
public class AlerteService : IAlerteService
{
    private readonly ApplicationDbContext _context;

    public AlerteService(ApplicationDbContext context) => _context = context;

    public async Task CreerAsync(TypeAlerte type, NiveauAlerte niveau, string titre, string message,
        long? chantierId = null, CancellationToken ct = default)
    {
        // Évite les doublons non lus identiques.
        bool existe = await _context.Alertes.AnyAsync(a =>
            !a.EstLue && a.Type == type && a.Titre == titre && a.ChantierId == chantierId, ct);
        if (existe) return;

        await _context.Alertes.AddAsync(new Alerte
        {
            Type = type, Niveau = niveau, Titre = titre, Message = message, ChantierId = chantierId
        }, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Alerte>> ObtenirNonLuesAsync(CancellationToken ct = default)
        => await _context.Alertes.Where(a => !a.EstLue)
            .OrderByDescending(a => a.CreatedAt).Take(50).ToListAsync(ct);

    public async Task EvaluerAlertesAsync(CancellationToken ct = default)
    {
        // Seuil budget faible : 15% restant (paramétrable via Parametres).
        double seuilPct = 15d;

        var budget = await _context.BudgetsComptes
            .Where(b => b.EstActif).OrderByDescending(b => b.Annee).FirstOrDefaultAsync(ct);
        if (budget is not null)
        {
            if (budget.MontantRestant <= 0)
                await CreerAsync(TypeAlerte.BudgetDepasse, NiveauAlerte.Critique,
                    "Budget Comptes épuisé", $"Le Budget Comptes {budget.Annee} est totalement consommé.", null, ct);
            else if (budget.PourcentageConsomme >= (100 - seuilPct))
                await CreerAsync(TypeAlerte.BudgetFaible, NiveauAlerte.Avertissement,
                    "Budget Comptes faible", $"Il reste {budget.MontantRestant:N0} Ar sur le Budget Comptes.", null, ct);
        }

        var chantiers = await _context.Chantiers.ToListAsync(ct);
        foreach (var c in chantiers)
        {
            if (c.BudgetMaterielRestant <= 0)
                await CreerAsync(TypeAlerte.BudgetDepasse, NiveauAlerte.Critique,
                    $"Budget Matériel épuisé - {c.Nom}", $"Le Budget Matériel du chantier {c.Nom} est épuisé.", c.Id, ct);
            else if (c.PourcentageConsomme >= (100 - seuilPct))
                await CreerAsync(TypeAlerte.BudgetFaible, NiveauAlerte.Avertissement,
                    $"Budget Matériel faible - {c.Nom}", $"Il reste {c.BudgetMaterielRestant:N0} Ar.", c.Id, ct);
        }

        var materiaux = await _context.Materiaux.ToListAsync(ct);
        foreach (var m in materiaux)
        {
            if (m.EstStockCritique)
                await CreerAsync(TypeAlerte.StockCritique, NiveauAlerte.Critique,
                    $"Stock critique - {m.Designation}", $"Stock épuisé pour {m.Designation}.", m.ChantierId, ct);
            else if (m.EstStockFaible)
                await CreerAsync(TypeAlerte.StockFaible, NiveauAlerte.Avertissement,
                    $"Stock faible - {m.Designation}", $"Stock disponible : {m.StockDisponible} {m.Unite}.", m.ChantierId, ct);

            // Réception qui approche le total commandé (ex : presque atteint les 900).
            if (m.PourcentageReception >= 90)
                await CreerAsync(TypeAlerte.Reception90, NiveauAlerte.Avertissement,
                    $"Réception presque complète - {m.Designation}",
                    $"{m.QuantiteRecue:N0} / {m.QuantiteCommandee:N0} {m.Unite} reçus ({m.PourcentageReception:N0}%) — le total commandé est presque atteint.",
                    m.ChantierId, ct);
        }
    }
}
