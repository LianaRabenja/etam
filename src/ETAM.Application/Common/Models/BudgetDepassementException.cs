namespace ETAM.Application.Common.Models;

/// <summary>
/// Levée lorsqu'une opération dépasse le budget disponible.
/// Le workflow propose alors l'utilisation de la réserve (validation Admin obligatoire).
/// </summary>
public class BudgetDepassementException : Exception
{
    public decimal Manquant { get; }
    public BudgetDepassementException(string message, decimal manquant) : base(message)
        => Manquant = manquant;
}
