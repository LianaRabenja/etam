using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Prévision journalière (module principal). Créée chaque jour par le chef de chantier,
/// elle contient plusieurs lignes et suit un workflow de validation à deux niveaux.
/// </summary>
public class PrevisionJournaliere : BaseEntity
{
    public long ChantierId { get; set; }
    public Chantier Chantier { get; set; } = null!;

    public DateTime DatePrevision { get; set; }
    public string Reference { get; set; } = null!;

    public StatutPrevision Statut { get; set; } = StatutPrevision.Brouillon;

    // --- Traçabilité du workflow ---
    public string? SoumisePar { get; set; }
    public DateTime? DateSoumission { get; set; }

    public string? ValideeParRfId { get; set; }
    public DateTime? DateValidationRf { get; set; }

    public string? ValideeParAdminId { get; set; }
    public DateTime? DateValidationAdmin { get; set; }

    public DateTime? DateExecution { get; set; }
    public string? MotifRefus { get; set; }

    public string? Observation { get; set; }

    public ICollection<PrevisionLigne> Lignes { get; set; } = new List<PrevisionLigne>();

    // --- Propriétés calculées ---
    public decimal Total => Lignes?.Sum(l => l.Total) ?? 0m;
    public bool EstModifiable =>
        Statut == StatutPrevision.Brouillon
        || Statut == StatutPrevision.Soumise
        || Statut == StatutPrevision.ValideeResponsableFinancier;
}
