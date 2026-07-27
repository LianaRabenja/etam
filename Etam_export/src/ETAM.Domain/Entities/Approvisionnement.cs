using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Bon d'approvisionnement saisi par le responsable / chef de chantier.
/// Liste les besoins du chantier ; une fois validé, il génère automatiquement
/// une Prévision journalière qui suivra le workflow de validation.
/// </summary>
public class Approvisionnement : BaseEntity
{
    public long ChantierId { get; set; }
    public Chantier Chantier { get; set; } = null!;

    public DateTime DateAppro { get; set; } = DateTime.UtcNow;
    public string Reference { get; set; } = null!;

    public StatutApprovisionnement Statut { get; set; } = StatutApprovisionnement.Brouillon;
    public string? Observation { get; set; }

    /// <summary>Prévision générée lors de la validation (le cas échéant).</summary>
    public long? PrevisionJournaliereId { get; set; }
    public PrevisionJournaliere? PrevisionJournaliere { get; set; }

    public ICollection<ApprovisionnementLigne> Lignes { get; set; } = new List<ApprovisionnementLigne>();

    public decimal Total => Lignes?.Sum(l => l.Total) ?? 0m;
    public bool EstModifiable => Statut == StatutApprovisionnement.Brouillon;
}
