using ETAM.Domain.Common;

namespace ETAM.Domain.Entities;

/// <summary>
/// Budget Comptes : budget ANNUEL et UNIQUE pour toute l'entreprise
/// (ex : Budget 2026 = 50 000 000 Ar). Finance toutes les dépenses générales
/// (carburant, nourriture, salaires, dettes matériaux, etc.).
/// </summary>
public class BudgetCompte : BaseEntity
{
    /// <summary>Exercice comptable (ex : 2026). Unique.</summary>
    public int Annee { get; set; }

    public string Libelle { get; set; } = null!;

    /// <summary>Plafond alloué au budget annuel (Ar). FIXE — sert de référence et adossé à la banque.</summary>
    public decimal MontantInitial { get; set; }

    /// <summary>
    /// Budget réel : montant transféré depuis la banque vers ce budget (part de 0 et augmente
    /// à chaque transfert). C'est ce pot qui finance réellement les prévisions et les dettes.
    /// </summary>
    public decimal MontantTransfere { get; set; }

    /// <summary>Montant réellement consommé (mis à jour à l'exécution des prévisions/dépenses).</summary>
    public decimal MontantConsomme { get; set; }

    /// <summary>Réserve globale de l'entreprise.</summary>
    public decimal Reserve { get; set; }
    public decimal ReserveUtilisee { get; set; }

    public bool EstActif { get; set; } = true;

    // --- Propriétés calculées ---
    public decimal MontantRestant => MontantInitial - MontantConsomme;
    public double PourcentageConsomme =>
        MontantInitial <= 0 ? 0 : (double)(MontantConsomme / MontantInitial) * 100d;
    public decimal ReserveRestante => Reserve - ReserveUtilisee;

    /// <summary>Budget réel encore disponible = transféré depuis la banque − consommé.</summary>
    public decimal DisponibleReel => MontantTransfere - MontantConsomme;
}
