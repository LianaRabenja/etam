using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>Alerte générée automatiquement (budget, stock, validation en attente...).</summary>
public class Alerte : BaseEntity
{
    public TypeAlerte Type { get; set; }
    public NiveauAlerte Niveau { get; set; } = NiveauAlerte.Info;

    public string Titre { get; set; } = null!;
    public string Message { get; set; } = null!;

    public long? ChantierId { get; set; }
    public Chantier? Chantier { get; set; }

    public bool EstLue { get; set; }
    public DateTime? DateLecture { get; set; }
}
