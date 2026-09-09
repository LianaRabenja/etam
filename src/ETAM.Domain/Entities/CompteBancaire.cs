using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Compte bancaire adossé soit à un chantier (finance son Budget Matériel),
/// soit au Budget Comptes annuel de l'entreprise. Le solde diminue au fur et à
/// mesure des exécutions de prévisions (retraits progressifs).
/// </summary>
public class CompteBancaire : BaseEntity
{
    public string Nom { get; set; } = null!;
    public string Banque { get; set; } = null!;
    public string? Numero { get; set; }
    public string Devise { get; set; } = "Ar";

    /// <summary>Solde courant du compte.</summary>
    public decimal Solde { get; set; }

    public bool EstActif { get; set; } = true;

    /// <summary>Nature du compte : rattaché à un chantier, ou dédié au Budget Comptes.</summary>
    public TypeCompteBancaire Type { get; set; } = TypeCompteBancaire.Chantier;

    /// <summary>Chantier propriétaire (null pour le compte du Budget Comptes).</summary>
    public long? ChantierId { get; set; }
    public Chantier? Chantier { get; set; }

    public ICollection<MouvementBancaire> Mouvements { get; set; } = new List<MouvementBancaire>();
}
