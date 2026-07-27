namespace ETAM.Domain.Enums;

/// <summary>
/// Type de budget impacté par une ligne de prévision ou une dépense.
/// Compte  = Budget Comptes annuel unique de l'entreprise.
/// Materiel = Budget Matériel propre à chaque chantier.
/// </summary>
public enum TypeBudget
{
    Compte = 0,
    Materiel = 1
}
