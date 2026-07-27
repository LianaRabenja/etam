using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Mouvement bancaire (dépôt, retrait, virement/paiement, frais).
/// Impacte le solde du compte : Dépôt = crédit, les autres = débit.
/// </summary>
public class MouvementBancaire : BaseEntity
{
    public long CompteBancaireId { get; set; }
    public CompteBancaire CompteBancaire { get; set; } = null!;

    public DateTime Date { get; set; } = DateTime.UtcNow;
    public TypeMouvementBancaire Type { get; set; }
    public decimal Montant { get; set; }

    public string? Beneficiaire { get; set; }
    public string? Motif { get; set; }
    public string? Reference { get; set; }

    /// <summary>
    /// Pour un transfert vers le budget demandé par le Responsable : false tant que
    /// l'Administrateur n'a pas validé (le solde/budget ne sont appliqués qu'à la validation).
    /// Les autres mouvements sont valides d'emblée.
    /// </summary>
    public bool EstValide { get; set; } = true;
    public string? DemandePar { get; set; }

    // Rattachements optionnels
    public long? ChantierId { get; set; }
    public Chantier? Chantier { get; set; }

    public long? FournisseurId { get; set; }
    public Fournisseur? Fournisseur { get; set; }

    public long? DetteFournisseurId { get; set; }
    public DetteFournisseur? DetteFournisseur { get; set; }

    /// <summary>Signe appliqué au solde : +1 pour un dépôt, -1 sinon.</summary>
    public int Sens => Type == TypeMouvementBancaire.Depot ? 1 : -1;
    public decimal MontantSigne => Montant * Sens;
}
