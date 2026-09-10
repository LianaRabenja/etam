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

            // Moitié du stock prévu déjà utilisée.
            if (m.QuantiteRecue > 0)
            {
                var pctUtilise = (double)(m.QuantiteUtilisee / m.QuantiteRecue) * 100d;
                if (pctUtilise >= 50)
                    await CreerAsync(TypeAlerte.SeuilMoitie, NiveauAlerte.Avertissement,
                        $"50 % consommé - {m.Designation}",
                        $"{m.QuantiteUtilisee:N0} / {m.QuantiteRecue:N0} {m.Unite} utilisés ({pctUtilise:N0} %). Stock restant : {m.StockDisponible:N0}.",
                        m.ChantierId, ct);
            }
        }

        await EvaluerSeuils50Async(ct);
        await EvaluerTravauxNonJustifiesAsync(ct);
    }

    /// <summary>
    /// Alerte dès que 50 % d'une enveloppe prévue est consommé, et alerte de dépassement
    /// au-delà de 100 %. Couvre : le budget projet du chantier, chaque rubrique de la
    /// prévision globale et chaque ligne prévue (ciment, main d'œuvre, etc.).
    /// </summary>
    private async Task EvaluerSeuils50Async(CancellationToken ct)
    {
        // Prévisions globales actives (validées / mises en banque) avec leurs lignes.
        var globales = await _context.PrevisionsGlobales
            .Include(p => p.Chantier).Include(p => p.Lignes)
            .Where(p => p.Statut == StatutPrevisionGlobale.ValideeAdministrateur
                     || p.Statut == StatutPrevisionGlobale.MiseEnBanque)
            .ToListAsync(ct);

        foreach (var g in globales)
        {
            var nomChantier = g.Chantier?.Nom ?? "chantier";
            var idsLignes = g.Lignes.Select(l => l.Id).ToList();
            if (idsLignes.Count == 0) continue;

            // Dépenses réellement engagées, par ligne globale (via les prévisions journalières
            // validées ou exécutées, rattachées aux lignes de la prévision globale).
            var consommeParLigne = await _context.PrevisionLignes
                .Where(pl => pl.PrevisionGlobaleLigneId != null
                          && idsLignes.Contains(pl.PrevisionGlobaleLigneId.Value)
                          && (pl.PrevisionJournaliere.Statut == StatutPrevision.Executee
                           || pl.PrevisionJournaliere.Statut == StatutPrevision.RapportSoumis
                           || pl.PrevisionJournaliere.Statut == StatutPrevision.Cloturee))
                .GroupBy(pl => pl.PrevisionGlobaleLigneId!.Value)
                .Select(grp => new { LigneId = grp.Key, Total = grp.Sum(x => x.Quantite * x.PrixUnitaireEstime) })
                .ToDictionaryAsync(x => x.LigneId, x => x.Total, ct);

            // --- 1) Par ligne prévue (ex : Ciment 2 100 000) ---
            foreach (var l in g.Lignes)
            {
                var prevu = l.Total;
                if (prevu <= 0) continue;
                consommeParLigne.TryGetValue(l.Id, out var consomme);
                var pct = (double)(consomme / prevu) * 100d;

                if (pct >= 100)
                    await CreerAsync(TypeAlerte.DepassementPrevision, NiveauAlerte.Critique,
                        $"Dépassement - {l.Designation} ({nomChantier})",
                        $"{l.Rubrique} / {l.Designation} : {consomme:N0} Ar dépensés sur {prevu:N0} Ar prévus ({pct:N0} %).",
                        g.ChantierId, ct);
                else if (pct >= 50)
                    await CreerAsync(TypeAlerte.SeuilMoitie, NiveauAlerte.Avertissement,
                        $"50 % atteint - {l.Designation} ({nomChantier})",
                        $"{l.Rubrique} / {l.Designation} : {consomme:N0} Ar dépensés sur {prevu:N0} Ar prévus ({pct:N0} %). Reste {prevu - consomme:N0} Ar.",
                        g.ChantierId, ct);
            }

            // --- 2) Par rubrique (ex : Approvisionnement 40 000 000, Main d'œuvre 10 000 000) ---
            foreach (var rub in g.Lignes.GroupBy(l => l.Rubrique))
            {
                var prevuRub = rub.Sum(l => l.Total);
                if (prevuRub <= 0) continue;
                var consommeRub = rub.Sum(l => consommeParLigne.TryGetValue(l.Id, out var v) ? v : 0m);
                var pctRub = (double)(consommeRub / prevuRub) * 100d;

                if (pctRub >= 100)
                    await CreerAsync(TypeAlerte.DepassementPrevision, NiveauAlerte.Critique,
                        $"Rubrique dépassée - {rub.Key} ({nomChantier})",
                        $"Rubrique {rub.Key} : {consommeRub:N0} Ar dépensés sur {prevuRub:N0} Ar prévus ({pctRub:N0} %).",
                        g.ChantierId, ct);
                else if (pctRub >= 50)
                    await CreerAsync(TypeAlerte.SeuilMoitie, NiveauAlerte.Avertissement,
                        $"Rubrique à 50 % - {rub.Key} ({nomChantier})",
                        $"Rubrique {rub.Key} : {consommeRub:N0} Ar dépensés sur {prevuRub:N0} Ar prévus ({pctRub:N0} %). Reste {prevuRub - consommeRub:N0} Ar.",
                        g.ChantierId, ct);
            }

            // --- 3) Budget projet global du chantier ---
            var prevuTotal = g.Total;
            if (prevuTotal > 0)
            {
                var consommeTotal = consommeParLigne.Values.Sum();
                var pctTotal = (double)(consommeTotal / prevuTotal) * 100d;

                if (pctTotal >= 100)
                    await CreerAsync(TypeAlerte.DepassementPrevision, NiveauAlerte.Critique,
                        $"Budget projet dépassé - {nomChantier}",
                        $"{consommeTotal:N0} Ar dépensés sur {prevuTotal:N0} Ar prévus ({pctTotal:N0} %).",
                        g.ChantierId, ct);
                else if (pctTotal >= 50)
                    await CreerAsync(TypeAlerte.SeuilMoitie, NiveauAlerte.Avertissement,
                        $"Budget projet à 50 % - {nomChantier}",
                        $"{consommeTotal:N0} Ar dépensés sur {prevuTotal:N0} Ar prévus ({pctTotal:N0} %). Reste {prevuTotal - consommeTotal:N0} Ar.",
                        g.ChantierId, ct);
            }
        }

        // --- 4) Budget Matériel de chaque chantier à 50 % ---
        foreach (var c in await _context.Chantiers.ToListAsync(ct))
        {
            if (c.BudgetMateriel > 0 && c.PourcentageConsomme >= 50 && c.PourcentageConsomme < 85)
                await CreerAsync(TypeAlerte.SeuilMoitie, NiveauAlerte.Avertissement,
                    $"Budget Matériel à 50 % - {c.Nom}",
                    $"{c.Consommation:N0} Ar consommés sur {c.BudgetMateriel:N0} Ar ({c.PourcentageConsomme:N0} %). Reste {c.BudgetMaterielRestant:N0} Ar.",
                    c.Id, ct);
        }

        // --- 5) Budget Comptes annuel à 50 % ---
        var bc = await _context.BudgetsComptes.Where(b => b.EstActif)
            .OrderByDescending(b => b.Annee).FirstOrDefaultAsync(ct);
        if (bc is not null && bc.PourcentageConsomme >= 50 && bc.PourcentageConsomme < 85)
            await CreerAsync(TypeAlerte.SeuilMoitie, NiveauAlerte.Avertissement,
                $"Budget Comptes à 50 % ({bc.Annee})",
                $"{bc.MontantConsomme:N0} Ar consommés ({bc.PourcentageConsomme:N0} %). Reste {bc.MontantRestant:N0} Ar.",
                null, ct);

        // --- 6) Enveloppe mensuelle ouverte à 50 % ---
        //     Le niveau qui manquait : on savait alerter sur le budget du projet et sur
        //     le Budget Matériel, mais pas sur le mois en cours — alors que c'est lui
        //     qui se vide le plus vite.
        var moisOuverts = await _context.PrevisionsMensuelles
            .Include(m => m.Chantier)
            .Where(m => m.Statut == StatutPrevisionMensuelle.Validee)
            .ToListAsync(ct);

        foreach (var m in moisOuverts)
        {
            if (m.EnveloppeTotale <= 0) continue;
            var nom = m.Chantier?.Nom ?? "chantier";

            if (m.MontantConsomme > m.EnveloppeTotale)
                await CreerAsync(TypeAlerte.DepassementPrevision, NiveauAlerte.Critique,
                    $"Enveloppe dépassée - {m.Libelle} ({nom})",
                    $"{m.MontantConsomme:N0} Ar décaissés sur une enveloppe de {m.EnveloppeTotale:N0} Ar " +
                    $"(dont {m.ReportMoisPrecedent:N0} Ar reportés). Dépassement de " +
                    $"{(m.MontantConsomme - m.EnveloppeTotale):N0} Ar.",
                    m.ChantierId, ct);
            else if (m.PourcentageConsomme >= 50)
                await CreerAsync(TypeAlerte.SeuilMoitie, NiveauAlerte.Avertissement,
                    $"Enveloppe du mois à {m.PourcentageConsomme:N0} % - {m.Libelle} ({nom})",
                    $"{m.MontantConsomme:N0} Ar décaissés sur {m.EnveloppeTotale:N0} Ar. " +
                    $"Reste {m.Disponible:N0} Ar pour finir le mois.",
                    m.ChantierId, ct);
        }

        // --- 7) Prévision journalière ouverte à 50 % ---
        //     L'argent est déjà sorti de la banque : ce qui compte ici est la part
        //     réellement distribuée par le chef, et ce qu'il lui reste en main.
        var joursOuverts = await _context.Previsions
            .Include(p => p.Chantier).Include(p => p.Lignes)
            .Where(p => p.Statut == StatutPrevision.Executee
                     || p.Statut == StatutPrevision.RapportSoumis)
            .ToListAsync(ct);

        foreach (var p in joursOuverts)
        {
            if (p.PlafondDuJour <= 0) continue;
            var nom = p.Chantier?.Nom ?? "chantier";

            if (p.MontantDecaisse > p.PlafondDuJour)
                await CreerAsync(TypeAlerte.DepassementPrevision, NiveauAlerte.Critique,
                    $"Plafond du jour dépassé - {p.Reference}",
                    $"{p.MontantDecaisse:N0} Ar sortis sur un plafond de {p.PlafondDuJour:N0} Ar " +
                    $"le {p.DatePrevision:dd/MM/yyyy} ({nom}).",
                    p.ChantierId, ct);
            else if (p.PourcentageDecaisse >= 50)
                await CreerAsync(TypeAlerte.SeuilMoitie, NiveauAlerte.Avertissement,
                    $"Prévision du jour à {p.PourcentageDecaisse:N0} % - {p.Reference}",
                    $"{p.MontantDecaisse:N0} Ar distribués sur {p.PlafondDuJour:N0} Ar " +
                    $"le {p.DatePrevision:dd/MM/yyyy} ({nom}). Reste {p.Reliquat:N0} Ar en main.",
                    p.ChantierId, ct);
        }
    }

    /// <summary>
    /// Signale l'argent sorti dont les travaux ne sont pas encore justifiés :
    /// prévision exécutée sans compte rendu, ou compte rendu non réceptionné par l'Administrateur.
    /// </summary>
    private async Task EvaluerTravauxNonJustifiesAsync(CancellationToken ct)
    {
        var enAttente = await _context.Previsions
            .Include(p => p.Chantier).Include(p => p.Lignes)
            .Where(p => p.Statut == StatutPrevision.Executee || p.Statut == StatutPrevision.RapportSoumis)
            .ToListAsync(ct);

        foreach (var p in enAttente)
        {
            var montant = p.Lignes.Sum(l => l.Quantite * l.PrixUnitaireEstime);
            var nom = p.Chantier?.Nom ?? "chantier";

            if (p.Statut == StatutPrevision.Executee)
                await CreerAsync(TypeAlerte.TravauxNonJustifies, NiveauAlerte.Avertissement,
                    $"Travaux à justifier - {p.Reference}",
                    $"{montant:N0} Ar dépensés le {p.DatePrevision:dd/MM/yyyy} sur {nom} : le chef doit rendre compte des travaux réalisés.",
                    p.ChantierId, ct);
            else
                await CreerAsync(TypeAlerte.TravauxNonJustifies, NiveauAlerte.Info,
                    $"Réception attendue - {p.Reference}",
                    $"Le compte rendu des travaux ({montant:N0} Ar, {nom}) attend votre réception.",
                    p.ChantierId, ct);
        }
    }
}
