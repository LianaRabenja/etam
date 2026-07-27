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
    ValidationEnAttente = 7
}
