using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Prévision journalière (module principal). Créée chaque jour par le chef de chantier,
/// elle contient plusieurs lignes et suit un workflow de validation à deux niveaux.
/// </summary>
public class PrevisionJournaliere : BaseEntity
{
    public long ChantierId { get; set; }
    public Chantier Chantier { get; set; } = null!;

    /// <summary>Enveloppe mensuelle sur laquelle cette journée s'impute.</summary>
    public long? PrevisionMensuelleId { get; set; }
    public PrevisionMensuelle? PrevisionMensuelle { get; set; }

    /// <summary>
    /// Journée du plan hebdomadaire à laquelle cette demande se rattache.
    /// Permet de comparer ce qui était prévu pour ce jour et ce qui a été demandé.
    /// </summary>
    public long? PlanJournalierId { get; set; }
    public PlanJournalier? PlanJournalier { get; set; }

    public DateTime DatePrevision { get; set; }
    public string Reference { get; set; } = null!;

    public StatutPrevision Statut { get; set; } = StatutPrevision.Brouillon;

    // --- Traçabilité du workflow ---
    public string? SoumisePar { get; set; }
    public DateTime? DateSoumission { get; set; }

    public string? ValideeParRfId { get; set; }
    public DateTime? DateValidationRf { get; set; }

    public string? ValideeParAdminId { get; set; }
    public DateTime? DateValidationAdmin { get; set; }

    public DateTime? DateExecution { get; set; }
    public string? MotifRefus { get; set; }

    public string? Observation { get; set; }

    // --- Compte rendu des travaux réalisés avec cette prévision ---
    /// <summary>Ce qui a réellement été fait avec l'argent de cette prévision (saisi par le chef).</summary>
    public string? RapportRealisation { get; set; }
    public DateTime? DateRapport { get; set; }

    /// <summary>Administrateur ayant réceptionné les travaux (re-validation finale).</summary>
    public string? RapportValideParId { get; set; }
    public DateTime? DateValidationRapport { get; set; }
    public string? MotifRefusRapport { get; set; }

    // --- Accusé de réception de l'argent mis à disposition ---
    /// <summary>
    /// Le chef de chantier atteste avoir reçu l'enveloppe du jour. Tant que ce n'est
    /// pas fait, aucun décaissement n'est possible : il ne peut pas dépenser un argent
    /// dont il n'a pas reconnu la réception.
    /// </summary>
    public string? AccuseReceptionParId { get; set; }
    public DateTime? DateAccuseReception { get; set; }

    /// <summary>Montant reconnu au moment de l'accusé, figé pour la trace écrite.</summary>
    public decimal? MontantAccuse { get; set; }

    /// <summary>Nom manuscrit ou saisi du signataire de l'accusé.</summary>
    public string? AccuseNomSignataire { get; set; }

    // --- Report de la veille ---
    /// <summary>
    /// Reliquat non décaissé de la prévision précédente du même chantier, figé au moment
    /// où celle-ci a été clôturée. S'ajoute au plafond du jour sans nouvelle sortie d'argent.
    /// </summary>
    public decimal ReportVeille { get; set; }

    /// <summary>Prévision d'où provient le report, pour remonter la chaîne des cumuls.</summary>
    public long? PrevisionPrecedenteId { get; set; }
    public PrevisionJournaliere? PrevisionPrecedente { get; set; }

    /// <summary>
    /// Total réellement décaissé sur cette journée. Mis à jour à chaque décaissement.
    /// </summary>
    public decimal MontantDecaisse { get; set; }

    public ICollection<PrevisionLigne> Lignes { get; set; } = new List<PrevisionLigne>();
    public ICollection<Decaissement> Decaissements { get; set; } = new List<Decaissement>();
    public ICollection<PieceJointe> PiecesJointes { get; set; } = new List<PieceJointe>();

    // --- Propriétés calculées ---

    /// <summary>Montant demandé pour la journée, hors report.</summary>
    public decimal Total => Lignes?.Sum(l => l.Total) ?? 0m;

    /// <summary>
    /// Plafond réellement dépensable aujourd'hui : ce qui a été demandé, plus le
    /// reliquat de la veille.
    /// </summary>
    public decimal PlafondDuJour => Total + ReportVeille;

    /// <summary>Ce qu'il reste à dépenser sur la journée.</summary>
    public decimal Reliquat => PlafondDuJour - MontantDecaisse;

    /// <summary>Part du plafond déjà consommée, pour les alertes de seuil.</summary>
    public double PourcentageDecaisse =>
        PlafondDuJour <= 0 ? 0 : (double)(MontantDecaisse / PlafondDuJour) * 100;

    /// <summary>Le chef a reconnu avoir reçu l'argent.</summary>
    public bool EstAccuseeReception => DateAccuseReception.HasValue;

    /// <summary>
    /// L'enveloppe est ouverte mais le chef n'a pas encore signé la réception :
    /// aucun décaissement n'est autorisé.
    /// </summary>
    public bool AttendAccuseReception =>
        Statut == StatutPrevision.Executee && !DateAccuseReception.HasValue;

    /// <summary>Des décaissements peuvent être saisis sur cette prévision.</summary>
    public bool PeutDecaisser =>
        Statut == StatutPrevision.Executee && DateAccuseReception.HasValue && Reliquat > 0;
    public bool EstModifiable =>
        Statut == StatutPrevision.Brouillon
        || Statut == StatutPrevision.Soumise
        || Statut == StatutPrevision.ValideeResponsableFinancier;

    /// <summary>Prévision exécutée dont les travaux ne sont pas encore réceptionnés par l'Administrateur.
    /// Tant qu'il en existe une, aucune nouvelle prévision ne peut être créée pour ce chantier.</summary>
    public bool BloqueNouvellePrevision =>
        Statut == StatutPrevision.Executee || Statut == StatutPrevision.RapportSoumis;

    /// <summary>Le chef doit rendre compte des travaux.</summary>
    public bool AttendRapport => Statut == StatutPrevision.Executee;

    /// <summary>L'Administrateur doit réceptionner les travaux.</summary>
    public bool AttendReceptionAdmin => Statut == StatutPrevision.RapportSoumis;
}
