namespace ETAM.Domain.Enums;

/// <summary>Types de mouvements sur un compte bancaire.</summary>
public enum TypeMouvementBancaire
{
    Depot = 0,      // Entrée d'argent (crédit)
    Retrait = 1,    // Sortie d'espèces (débit)
    Virement = 2,   // Paiement / virement sortant (débit)
    Frais = 3       // Frais bancaires (débit)
}
