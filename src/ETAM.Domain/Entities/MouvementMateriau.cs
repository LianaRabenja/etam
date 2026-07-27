using ETAM.Domain.Common;

namespace ETAM.Domain.Entities;

/// <summary>
/// Enregistre chaque mouvement (entrée/sortie) d'un matériau.
/// Permet de maintenir un historique complet des stocks.
/// </summary>
public class MouvementMateriau : BaseEntity
{
    public long MateriauxId { get; set; }
    public Materiau Materiau { get; set; } = null!;

    public DateTime DateMouvement { get; set; } = DateTime.UtcNow;

    /// <summary>Ancien champ texte (conservé pour compatibilité, non utilisé dans la fiche).</summary>
    public string? BesoinOuObjectif { get; set; }

    /// <summary>Quantité entrée (réception). Null ou 0 si pas d'entrée.</summary>
    public decimal QuantiteEntree { get; set; }

    /// <summary>Quantité sortie (utilisation). Null ou 0 si pas de sortie.</summary>
    public decimal QuantiteSortie { get; set; }

    /// <summary>Motif du mouvement (e.g., "LINTEAUX", "POTEAUX", "Utilisation", etc.)</summary>
    public string? Motif { get; set; }

    /// <summary>Solde sur besoin = besoin - cumul des entrées (ce qui reste à recevoir).</summary>
    public decimal SoldeSurBesoin { get; set; }

    /// <summary>Solde en stock après ce mouvement.</summary>
    public decimal SoldeEnStock { get; set; }
}
