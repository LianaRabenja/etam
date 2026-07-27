using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Ligne d'une prévision journalière. Le type de budget détermine quel budget
/// sera diminué lors de l'exécution (Compte -> Budget Comptes, Materiel -> Budget Matériel + stock).
/// </summary>
public class PrevisionLigne : BaseEntity
{
    public long PrevisionJournaliereId { get; set; }
    public PrevisionJournaliere PrevisionJournaliere { get; set; } = null!;

    public string Designation { get; set; } = null!;
    public string Categorie { get; set; } = null!;
    public TypeBudget TypeBudget { get; set; }

    /// <summary>Matériau ciblé lorsque la ligne est de type Materiel (utilisation de stock).</summary>
    public long? MateriauId { get; set; }
    public Materiau? Materiau { get; set; }

    /// <summary>
    /// Dette fournisseur ciblée : si renseignée, cette ligne est un remboursement de dette.
    /// À l'exécution, la dette diminue du montant de la ligne (imputé au Budget Comptes).
    /// </summary>
    public long? DetteFournisseurId { get; set; }
    public DetteFournisseur? DetteFournisseur { get; set; }

    /// <summary>
    /// Ligne de la prévision globale du projet à laquelle cette dépense est rattachée
    /// (ex : « APPRO / Ciment » dont l'enveloppe est 2 100 000). Permet de suivre
    /// la consommation de l'enveloppe et de détecter les dépassements.
    /// </summary>
    public long? PrevisionGlobaleLigneId { get; set; }
    public PrevisionGlobaleLigne? PrevisionGlobaleLigne { get; set; }

    public decimal Quantite { get; set; }
    public decimal PrixUnitaireEstime { get; set; }
    public string? Observation { get; set; }

    // --- Propriété calculée ---
    public decimal Total => Quantite * PrixUnitaireEstime;
}
