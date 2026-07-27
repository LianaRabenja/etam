using ETAM.Domain.Common;

namespace ETAM.Domain.Entities;

/// <summary>Fournisseur (matériaux, prestations...).</summary>
public class Fournisseur : BaseEntity
{
    public string Nom { get; set; } = null!;
    public string? Contact { get; set; }
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
    public string? Nif { get; set; }

    public ICollection<DetteFournisseur> Dettes { get; set; } = new List<DetteFournisseur>();
}
