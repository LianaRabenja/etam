namespace ETAM.Domain.Enums;

/// <summary>
/// Workflow d'une prévision journalière :
/// Brouillon -> Soumise -> Validée RF -> Validée Admin -> Exécutée (ou Refusée).
/// </summary>
public enum StatutPrevision
{
    Brouillon = 0,
    Soumise = 1,
    ValideeResponsableFinancier = 2,
    ValideeAdministrateur = 3,
    Executee = 4,
    Refusee = 5
}
