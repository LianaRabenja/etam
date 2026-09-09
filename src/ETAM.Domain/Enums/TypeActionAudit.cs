namespace ETAM.Domain.Enums;

/// <summary>Types d'actions tracées dans le journal d'audit.</summary>
public enum TypeActionAudit
{
    Connexion = 0,
    Deconnexion = 1,
    Ajout = 2,
    Modification = 3,
    Suppression = 4,
    Validation = 5,
    Execution = 6,
    Refus = 7
}
