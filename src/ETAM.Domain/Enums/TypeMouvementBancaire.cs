namespace ETAM.Domain.Enums;

/// <summary>Types de mouvements sur un compte bancaire.</summary>
public enum TypeMouvementBancaire
{
    Depot = 0,      // Entrée d'argent (crédit)
    Retrait = 1,    // Sortie d'espèces (débit)
    Virement = 2,   // Paiement / virement sortant (débit)
    Frais = 3,      // Frais bancaires (débit)

    /// <summary>
    /// Fléchage : affectation d'une partie du solde à un budget (Matériel ou Comptes).
    /// L'argent RESTE en banque — aucun débit. Seule une sortie d'argent réelle
    /// (exécution d'une prévision) fait baisser le solde.
    /// </summary>
    Flechage = 4
}
