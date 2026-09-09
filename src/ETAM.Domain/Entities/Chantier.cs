using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Chantier : élément central de l'ERP (ex : Ampirika, Ambovombe, Tuléar).
/// Porte son propre Budget Matériel et sa réserve.
/// </summary>
public class Chantier : BaseEntity
{
    public string Nom { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Localisation { get; set; }
    public string? Responsable { get; set; }

    public DateTime DateDebut { get; set; }
    public DateTime? DateFin { get; set; }

    public StatutChantier Statut { get; set; } = StatutChantier.EnPreparation;

    // --- Marché (en Ariary) ---
    /// <summary>Montant total du marché signé avec le client (ex : 150 000 000).</summary>
    public decimal MontantMarche { get; set; }

    /// <summary>Marge conservée par l'entreprise (ex : 80 000 000).</summary>
    public decimal Benefice { get; set; }

    // --- Budget Matériel propre au chantier (en Ariary) ---
    /// <summary>Plafond du Budget Matériel (FIXE, adossé au compte bancaire du chantier).</summary>
    public decimal BudgetMateriel { get; set; }
    public decimal Reserve { get; set; }
    public decimal ReserveUtilisee { get; set; }
    public decimal Consommation { get; set; }

    /// <summary>Budget Matériel réel transféré depuis la banque du chantier (part de 0, augmente aux transferts).</summary>
    public decimal MaterielTransfere { get; set; }

    public double PourcentageAvancement { get; set; }
    public string? Observation { get; set; }

    // --- Navigation ---
    public ICollection<Materiau> Materiaux { get; set; } = new List<Materiau>();
    public ICollection<PrevisionJournaliere> Previsions { get; set; } = new List<PrevisionJournaliere>();
    public ICollection<PrevisionMensuelle> PrevisionsMensuelles { get; set; } = new List<PrevisionMensuelle>();
    public ICollection<Depense> Depenses { get; set; } = new List<Depense>();

    // --- Propriétés calculées (non mappées) ---
    /// <summary>Budget réellement affecté au chantier = marché − bénéfice (ex : 150 M − 80 M = 70 M).
    /// C'est le montant que doit couvrir la prévision globale du projet.</summary>
    public decimal BudgetProjet => MontantMarche - Benefice;

    public decimal BudgetMaterielRestant => BudgetMateriel - Consommation;
    public decimal ReserveRestante => Reserve - ReserveUtilisee;
    public double PourcentageConsomme =>
        BudgetMateriel <= 0 ? 0 : (double)(Consommation / BudgetMateriel) * 100d;

    /// <summary>Budget Matériel réel encore disponible = transféré depuis la banque − consommé.</summary>
    public decimal MaterielDisponible => MaterielTransfere - Consommation;
}
