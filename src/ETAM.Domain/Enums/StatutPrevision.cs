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
    Refusee = 5,

    /// <summary>Le chef a rendu compte des travaux réalisés ; en attente de la re-validation de l'Administrateur.</summary>
    RapportSoumis = 6,

    /// <summary>Travaux réceptionnés par l'Administrateur : le cycle est clos, une nouvelle prévision est possible.</summary>
    Cloturee = 7
}
