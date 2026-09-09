using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Sortie d'argent réelle, au fil de l'eau, à l'intérieur d'une prévision journalière.
///
/// C'est le SEUL évènement qui débite le compte bancaire et augmente la consommation
/// du chantier et du mois. L'ouverture d'une prévision journalière, elle, ne fait
/// qu'autoriser un plafond de dépense pour la journée.
///
/// Exemple : prévision du lundi = 5 000 000 Ar. Un maçon demande 190 000 Ar :
/// un décaissement de 190 000 Ar est enregistré, la banque perd 190 000 Ar,
/// et les 4 810 000 Ar restants se reportent sur la journée suivante.
/// </summary>
public class Decaissement : BaseEntity
{
    public long PrevisionJournaliereId { get; set; }
    public PrevisionJournaliere PrevisionJournaliere { get; set; } = null!;

    /// <summary>Ligne de la prévision que ce paiement règle, quand elle est identifiée.</summary>
    public long? PrevisionLigneId { get; set; }
    public PrevisionLigne? PrevisionLigne { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    /// <summary>Personne ou entreprise qui a reçu l'argent.</summary>
    public string Beneficiaire { get; set; } = null!;

    /// <summary>Ce que paie ce décaissement, en clair.</summary>
    public string Motif { get; set; } = null!;

    public decimal Montant { get; set; }

    public ModePaiement Mode { get; set; } = ModePaiement.Especes;

    /// <summary>Compte débité. Obligatoire : c'est de là que sort l'argent.</summary>
    public long CompteBancaireId { get; set; }
    public CompteBancaire CompteBancaire { get; set; } = null!;

    /// <summary>Budget impacté : Matériel (chantier) ou Compte (entreprise).</summary>
    public TypeBudget BudgetConcerne { get; set; } = TypeBudget.Materiel;

    /// <summary>Numéro de chèque, référence de virement, numéro de facture...</summary>
    public string? Reference { get; set; }

    /// <summary>Nom du bénéficiaire qui atteste avoir reçu la somme.</summary>
    public string? AccuseNom { get; set; }
    public DateTime? DateAccuse { get; set; }

    public string? Observation { get; set; }

    /// <summary>Justificatifs de ce paiement (facture, reçu photographié).</summary>
    public ICollection<PieceJointe> PiecesJointes { get; set; } = new List<PieceJointe>();

    /// <summary>Le bénéficiaire a signé le reçu.</summary>
    public bool EstAccuse => DateAccuse.HasValue;
}
