namespace ETAM.Domain.Enums;

/// <summary>Moyen par lequel l'argent est sorti pour un décaissement.</summary>
public enum ModePaiement
{
    Especes = 0,
    Cheque = 1,
    Virement = 2,
    MobileMoney = 3
}
