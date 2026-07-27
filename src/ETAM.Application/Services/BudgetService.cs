using ETAM.Application.Common.Models;
using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Interfaces;

namespace ETAM.Application.Services;

/// <summary>
/// Gestion du Budget Comptes annuel unique. Applique la règle de blocage
/// en cas de dépassement et l'utilisation contrôlée de la réserve.
/// </summary>
public class BudgetService : IBudgetService
{
    private readonly IUnitOfWork _uow;

    public BudgetService(IUnitOfWork uow) => _uow = uow;

    public async Task<BudgetCompte?> ObtenirBudgetCompteActifAsync(CancellationToken ct = default)
    {
        var budgets = await _uow.BudgetsComptes.ListAsync(b => b.EstActif && !b.IsDeleted, ct);
        return budgets.OrderByDescending(b => b.Annee).FirstOrDefault();
    }

    public async Task<decimal> VerifierBudgetCompteAsync(decimal montant, CancellationToken ct = default)
    {
        var budget = await ObtenirBudgetCompteActifAsync(ct)
            ?? throw new InvalidOperationException("Aucun Budget Comptes actif n'est défini.");
        var depassement = montant - budget.MontantRestant;
        return depassement > 0 ? depassement : 0m;
    }

    public async Task<Result> ImputerBudgetCompteAsync(decimal montant, bool utiliserReserve, CancellationToken ct = default)
    {
        var budget = await ObtenirBudgetCompteActifAsync(ct);
        if (budget is null)
            return Result.Failure("Aucun Budget Comptes actif n'est défini.");

        var depassement = montant - budget.MontantRestant;

        if (depassement > 0)
        {
            // Budget dépassé -> blocage sauf recours explicite à la réserve.
            if (!utiliserReserve)
                return Result.Failure(
                    $"Budget Comptes dépassé de {depassement:N0} Ar. Validation Administrateur et réserve requises.");

            if (depassement > budget.ReserveRestante)
                return Result.Failure(
                    $"Réserve insuffisante. Manque {(depassement - budget.ReserveRestante):N0} Ar.");

            budget.ReserveUtilisee += depassement;
            budget.MontantConsomme = budget.MontantInitial; // budget saturé
        }
        else
        {
            budget.MontantConsomme += montant;
        }

        _uow.BudgetsComptes.Update(budget);
        return Result.Success();
    }
}
