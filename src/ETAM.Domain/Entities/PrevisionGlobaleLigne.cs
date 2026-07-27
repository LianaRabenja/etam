using ETAM.Domain.Common;

namespace ETAM.Domain.Entities;

/// <summary>
/// Ligne d'une prévision globale. La « rubrique » est libre (créée par l'utilisateur) :
/// ex. « Approvisionnement », « Main d'œuvre », « Matériel », « Transport »...
/// Exemple : Rubrique = Approvisionnement, Désignation = Ciment, Unité = t, Quantité = 3, Prix = 30 000.
/// </summary>
public class PrevisionGlobaleLigne : BaseEntity
{
    public long PrevisionGlobaleId { get; set; }
    public PrevisionGlobale PrevisionGlobale { get; set; } = null!;

    /// <summary>Rubrique / poste de dépense (libre). Ex : « Approvisionnement », « Main d'œuvre ».</summary>
    public string Rubrique { get; set; } = null!;

    public string Designation { get; set; } = null!;
    public string? Unite { get; set; }
    public decimal Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public string? Observation { get; set; }

    public decimal Total => Quantite * PrixUnitaire;
}
