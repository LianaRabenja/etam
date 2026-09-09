using ETAM.Domain.Common;

namespace ETAM.Domain.Entities;

/// <summary>
/// Article du catalogue : désignation pré-enregistrée avec son prix unitaire de référence.
/// Sert à alimenter l'autocomplétion des approvisionnements et à pré-remplir le prix.
/// </summary>
public class ArticleCatalogue : BaseEntity
{
    public string Designation { get; set; } = null!;
    public string? Categorie { get; set; }
    public string? Unite { get; set; }
    public decimal PrixUnitaire { get; set; }
}
