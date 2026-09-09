namespace ETAM.Domain.Enums;

/// <summary>Catégories d'alertes générées automatiquement par le système.</summary>
public enum TypeAlerte
{
    BudgetFaible = 0,
    BudgetDepasse = 1,
    ReserveUtilisee = 2,
    StockFaible = 3,
    StockCritique = 4,
    Reception90 = 5,
    PrevisionEnAttente = 6,
    ValidationEnAttente = 7,

    /// <summary>La moitié (50 %) d'une enveloppe est consommée : budget, rubrique, ligne prévue ou stock.</summary>
    SeuilMoitie = 8,

    /// <summary>Une enveloppe prévue est dépassée (dépense supérieure au prévisionnel).</summary>
    DepassementPrevision = 9,

    /// <summary>Des travaux financés attendent le compte rendu ou la réception de l'Administrateur.</summary>
    TravauxNonJustifies = 10
}
