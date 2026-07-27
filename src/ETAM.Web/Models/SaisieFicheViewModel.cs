using System.ComponentModel.DataAnnotations;

namespace ETAM.Web.Models;

/// <summary>Saisie de plusieurs lignes de mouvement sur une même fiche (un seul article).</summary>
public class SaisieFicheViewModel
{
    [Required(ErrorMessage = "L'article est obligatoire.")]
    public long MateriauxId { get; set; }

    public List<LigneMouvementViewModel> Lignes { get; set; } = new();
}

public class LigneMouvementViewModel
{
    public DateTime? DateMouvement { get; set; }
    public decimal QuantiteEntree { get; set; }
    public decimal QuantiteSortie { get; set; }
    public string? Motif { get; set; }
}
