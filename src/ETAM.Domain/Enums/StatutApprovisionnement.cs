namespace ETAM.Domain.Enums;

/// <summary>
/// Cycle de vie d'un approvisionnement saisi par le responsable de chantier.
/// Une fois validé, il est converti en Prévision journalière.
/// </summary>
public enum StatutApprovisionnement
{
    Brouillon = 0,
    Valide = 1,     // Converti en prévision
    Annule = 2
}
