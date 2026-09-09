namespace ETAM.Domain.Enums;

/// <summary>Nature d'un compte bancaire : rattaché à un chantier ou au Budget Comptes.</summary>
public enum TypeCompteBancaire
{
    Chantier = 0,   // Compte propre à un chantier (adossé au Budget Matériel)
    Comptes = 1     // Compte dédié au Budget Comptes annuel de l'entreprise
}
