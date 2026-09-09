using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Prévision globale d'un projet de chantier : le budget prévisionnel total,
/// détaillé par rubriques libres (Approvisionnement, Main d'œuvre, Matériel, ...).
/// Une fois validée, son montant total est mis en banque (compte du chantier),
/// puis consommé petit à petit via les approvisionnements et prévisions journalières.
/// </summary>
public class PrevisionGlobale : BaseEntity
{
    public long ChantierId { get; set; }
    public Chantier Chantier { get; set; } = null!;

    public string Reference { get; set; } = null!;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public StatutPrevisionGlobale Statut { get; set; } = StatutPrevisionGlobale.Brouillon;
    public string? Observation { get; set; }

    // --- Workflow ---
    public string? SoumisePar { get; set; }
    public DateTime? DateSoumission { get; set; }
    public string? ValideeParRfId { get; set; }
    public DateTime? DateValidationRf { get; set; }
    public string? ValideeParAdminId { get; set; }
    public DateTime? DateValidationAdmin { get; set; }
    public string? MotifRefus { get; set; }
    public DateTime? DateMiseEnBanque { get; set; }

    public ICollection<PrevisionGlobaleLigne> Lignes { get; set; } = new List<PrevisionGlobaleLigne>();

    public decimal Total => Lignes?.Sum(l => l.Total) ?? 0m;

    /// <summary>Modifiable tant qu'elle est en brouillon ou refusée.</summary>
    public bool EstModifiable =>
        Statut == StatutPrevisionGlobale.Brouillon || Statut == StatutPrevisionGlobale.Refusee;

    /// <summary>Totalement validée (prête à être mise en banque).</summary>
    public bool EstValidee => Statut == StatutPrevisionGlobale.ValideeAdministrateur;
}
