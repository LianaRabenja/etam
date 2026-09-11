using ETAM.Domain.Common;

namespace ETAM.Domain.Entities;

/// <summary>
/// Matériau rattaché DIRECTEMENT à un chantier (il n'existe pas de magasin central).
/// </summary>
public class Materiau : BaseEntity
{
    public long ChantierId { get; set; }
    public Chantier Chantier { get; set; } = null!;

    public string Categorie { get; set; } = null!;
    public string Designation { get; set; } = null!;
    public string Unite { get; set; } = null!;

    /// <summary>Localité / sous-site de la fiche (ex : "Centre", "Église"). Facultatif.</summary>
    public string? Localite { get; set; }

    /// <summary>Besoin ou objectif total de l'article (quantité totale nécessaire, ex : 669).</summary>
    public decimal Besoin { get; set; }

    /// <summary>Obsolète : la notion de « commandé » a été retirée du suivi de stock.
    /// La colonne est conservée en base pour ne pas casser l'historique, mais elle
    /// n'est plus saisie ni affichée. Le suivi se fait sur Besoin → Reçu → Utilisé.</summary>
    public decimal QuantiteCommandee { get; set; }

    public decimal QuantiteRecue { get; set; }
    public decimal QuantiteUtilisee { get; set; }

    public decimal SeuilMinimal { get; set; }
    public decimal PrixUnitaire { get; set; }

    // --- Propriétés calculées ---
    /// <summary>Stock disponible = Quantité reçue - Quantité utilisée.</summary>
    public decimal StockDisponible => QuantiteRecue - QuantiteUtilisee;
    public decimal ValeurRestante => StockDisponible * PrixUnitaire;
    /// <summary>Avancement de la réception, mesuré sur l'objectif (besoin total).</summary>
    public double PourcentageReception =>
        Besoin <= 0 ? 0 : (double)(QuantiteRecue / Besoin) * 100d;

    /// <summary>Ce qu'il reste à recevoir pour atteindre l'objectif.</summary>
    public decimal ResteARecevoir => Besoin - QuantiteRecue > 0 ? Besoin - QuantiteRecue : 0;
    public bool EstStockFaible => StockDisponible <= SeuilMinimal && StockDisponible > 0;
    public bool EstStockCritique => StockDisponible <= 0;
}
