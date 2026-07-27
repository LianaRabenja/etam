namespace ETAM.Domain.Enums;

/// <summary>Cycle de vie d'un chantier de forage / BTP.</summary>
public enum StatutChantier
{
    EnPreparation = 0,
    EnCours = 1,
    Suspendu = 2,
    Termine = 3,
    Cloture = 4
}
