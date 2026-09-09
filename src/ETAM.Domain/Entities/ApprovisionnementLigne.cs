using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Ligne d'un bon d'approvisionnement. Sa structure reflète celle d'une ligne
/// de prévision : le type de budget (Compte / Matériel) est défini par ligne,
/// ce qui déterminera l'impact lors de l'exécution de la prévision générée.
/// </summary>
public class ApprovisionnementLigne : BaseEntity
{
    public long ApprovisionnementId { get; set; }
    public Approvisionnement Approvisionnement { get; set; } = null!;

    public string Designation { get; set; } = null!;
    public string Categorie { get; set; } = null!;
    public TypeBudget TypeBudget { get; set; }

    public long? MateriauId { get; set; }
    public Materiau? Materiau { get; set; }

    /// <summary>Dette fournisseur ciblée (ligne de remboursement de dette).</summary>
    public long? DetteFournisseurId { get; set; }
    public DetteFournisseur? DetteFournisseur { get; set; }

    public decimal Quantite { get; set; }
    public decimal PrixUnitaireEstime { get; set; }
    public string? Observation { get; set; }

    public decimal Total => Quantite * PrixUnitaireEstime;
}
