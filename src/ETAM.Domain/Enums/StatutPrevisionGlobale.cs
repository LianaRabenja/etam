namespace ETAM.Domain.Enums;

/// <summary>
/// Workflow d'une prévision globale de projet :
/// Brouillon -> Soumise -> Validée RF -> Validée Admin -> Mise en banque.
/// </summary>
public enum StatutPrevisionGlobale
{
    Brouillon = 0,
    Soumise = 1,
    ValideeResponsableFinancier = 2,
    ValideeAdministrateur = 3,
    MiseEnBanque = 4,
    Refusee = 5
}
