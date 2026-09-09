using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Dépense réellement effectuée. Peut être issue d'une prévision validée.
/// Met automatiquement à jour le budget concerné.
/// </summary>
public class Depense : BaseEntity
{
    public DateTime Date { get; set; }

    public long ChantierId { get; set; }
    public Chantier Chantier { get; set; } = null!;

    /// <summary>Prévision d'origine (optionnelle).</summary>
    public long? PrevisionJournaliereId { get; set; }
    public PrevisionJournaliere? PrevisionJournaliere { get; set; }

    public string Categorie { get; set; } = null!;
    public string Designation { get; set; } = null!;

    public decimal Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }

    public TypeBudget BudgetConcerne { get; set; }

    public string? Justificatif { get; set; }
    public string? Observation { get; set; }

    // --- Propriété calculée ---
    public decimal Montant => Quantite * PrixUnitaire;
}
