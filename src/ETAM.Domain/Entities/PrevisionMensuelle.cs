using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Enveloppe mensuelle d'un chantier (ex : 125 000 000 Ar pour août 2026).
/// Niveau intermédiaire entre la prévision globale du projet et les prévisions
/// journalières : la somme des mois ne peut pas dépasser le budget projet, et
/// chaque prévision journalière s'impute sur le mois en cours.
///
/// L'argent ne quitte PAS la banque à l'ouverture du mois : l'enveloppe est un
/// plafond d'autorisation. Le compte n'est débité qu'au fil des décaissements réels.
/// </summary>
public class PrevisionMensuelle : BaseEntity
{
    public long ChantierId { get; set; }
    public Chantier Chantier { get; set; } = null!;

    /// <summary>Plan projet auquel ce mois se rattache (facultatif mais recommandé).</summary>
    public long? PrevisionGlobaleId { get; set; }
    public PrevisionGlobale? PrevisionGlobale { get; set; }

    public int Annee { get; set; }

    /// <summary>Mois de 1 (janvier) à 12 (décembre).</summary>
    public int Mois { get; set; }

    /// <summary>Référence lisible, ex : PMENS-NOS-01-202608.</summary>
    public string Reference { get; set; } = null!;

    /// <summary>Montant décidé pour ce mois, hors report (ex : 125 000 000).</summary>
    public decimal MontantPrevu { get; set; }

    /// <summary>
    /// Reliquat non consommé du mois précédent, figé à la clôture de celui-ci.
    /// Il s'ajoute au montant prévu sans jamais modifier le budget du projet.
    /// </summary>
    public decimal ReportMoisPrecedent { get; set; }

    /// <summary>
    /// Total réellement décaissé sur ce mois. Mis à jour à chaque décaissement,
    /// jamais à l'ouverture d'une prévision journalière.
    /// </summary>
    public decimal MontantConsomme { get; set; }

    /// <summary>Mois précédent dont provient le report, pour remonter la chaîne des cumuls.</summary>
    public long? PrevisionMensuellePrecedenteId { get; set; }
    public PrevisionMensuelle? PrevisionMensuellePrecedente { get; set; }

    public StatutPrevisionMensuelle Statut { get; set; } = StatutPrevisionMensuelle.Brouillon;

    public string? SoumisePar { get; set; }
    public DateTime? DateSoumission { get; set; }
    public string? ValideeParId { get; set; }
    public DateTime? DateValidation { get; set; }
    public string? MotifRefus { get; set; }

    public DateTime? DateCloture { get; set; }
    public string? ClotureeParId { get; set; }

    public string? Observation { get; set; }

    public ICollection<PrevisionMensuelleLigne> Lignes { get; set; } = new List<PrevisionMensuelleLigne>();
    public ICollection<PrevisionJournaliere> PrevisionsJournalieres { get; set; } = new List<PrevisionJournaliere>();

    // --- Propriétés calculées ---

    /// <summary>Ce dont dispose réellement le mois : montant décidé + report du mois précédent.</summary>
    public decimal EnveloppeTotale => MontantPrevu + ReportMoisPrecedent;

    /// <summary>Ce qu'il reste à décaisser sur le mois.</summary>
    public decimal Disponible => EnveloppeTotale - MontantConsomme;

    /// <summary>Part de l'enveloppe déjà consommée, pour les alertes de seuil.</summary>
    public double PourcentageConsomme =>
        EnveloppeTotale <= 0 ? 0 : (double)(MontantConsomme / EnveloppeTotale) * 100;

    /// <summary>Somme des lignes de répartition, quand le mois a été détaillé par rubrique.</summary>
    public decimal TotalLignes => Lignes?.Sum(l => l.Montant) ?? 0m;

    /// <summary>Le mois accepte de nouvelles prévisions journalières.</summary>
    public bool EstOuvert => Statut == StatutPrevisionMensuelle.Validee;

    public bool EstModifiable => Statut == StatutPrevisionMensuelle.Brouillon;

    /// <summary>Libellé du mois en français, ex : « août 2026 ».</summary>
    public string Libelle => $"{NomDuMois(Mois)} {Annee}";

    public static string NomDuMois(int mois) => mois switch
    {
        1 => "janvier", 2 => "février", 3 => "mars", 4 => "avril",
        5 => "mai", 6 => "juin", 7 => "juillet", 8 => "août",
        9 => "septembre", 10 => "octobre", 11 => "novembre", 12 => "décembre",
        _ => "mois " + mois
    };
}
