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

    public decimal QuantiteCommandee { get; set; }
    public decimal QuantiteRecue { get; set; }
    public decimal QuantiteUtilisee { get; set; }

    public decimal SeuilMinimal { get; set; }
    public decimal PrixUnitaire { get; set; }

    // --- Propriétés calculées ---
    /// <summary>Stock disponible = Quantité reçue - Quantité utilisée.</summary>
    public decimal StockDisponible => QuantiteRecue - QuantiteUtilisee;
    public decimal ValeurRestante => StockDisponible * PrixUnitaire;
    public double PourcentageReception =>
        QuantiteCommandee <= 0 ? 0 : (double)(QuantiteRecue / QuantiteCommandee) * 100d;
    public bool EstStockFaible => StockDisponible <= SeuilMinimal && StockDisponible > 0;
    public bool EstStockCritique => StockDisponible <= 0;
}
