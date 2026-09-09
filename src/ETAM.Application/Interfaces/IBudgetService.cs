using ETAM.Application.Common.Models;
using ETAM.Domain.Entities;

namespace ETAM.Application.Interfaces;

public interface IBudgetService
{
    Task<BudgetCompte?> ObtenirBudgetCompteActifAsync(CancellationToken ct = default);

    /// <summary>
    /// Vérifie qu'un montant peut être imputé au Budget Comptes.
    /// Retourne l'éventuel dépassement (valeur > 0 = réserve nécessaire).
    /// </summary>
    Task<decimal> VerifierBudgetCompteAsync(decimal montant, CancellationToken ct = default);

    /// <summary>Impute un montant au Budget Comptes (option réserve, validation Admin obligatoire).</summary>
    Task<Result> ImputerBudgetCompteAsync(decimal montant, bool utiliserReserve, CancellationToken ct = default);
}
