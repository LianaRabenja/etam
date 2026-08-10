namespace ETAM.Domain.Enums;

/// <summary>Cycle de vie d'une enveloppe mensuelle de chantier.</summary>
public enum StatutPrevisionMensuelle
{
    /// <summary>En cours de saisie, aucune prévision journalière ne peut s'y rattacher.</summary>
    Brouillon = 0,

    /// <summary>Ouverte : les prévisions journalières du mois s'imputent dessus.</summary>
    Validee = 1,

    /// <summary>Mois terminé. Le reliquat a été reporté sur le mois suivant.</summary>
    Cloturee = 2,

    /// <summary>Refusée par l'Administrateur.</summary>
    Refusee = 3
}
