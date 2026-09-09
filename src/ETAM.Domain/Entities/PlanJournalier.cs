using ETAM.Domain.Common;

namespace ETAM.Domain.Entities;

/// <summary>
/// Ce qu'on prévoit de dépenser un jour donné sur un chantier.
///
/// Se saisit par semaine, à l'intérieur de l'enveloppe du mois : le lundi, on écrit
/// ce que chaque journée de la semaine devrait coûter. C'est un PLAN, pas une
/// autorisation : aucun argent n'y est rattaché tant qu'aucune demande n'est faite.
///
/// Le chantier envoie ensuite ses demandes, qui se comparent à ce plan.
/// </summary>
public class PlanJournalier : BaseEntity
{
    /// <summary>Enveloppe du mois à laquelle ce jour appartient.</summary>
    public long PrevisionMensuelleId { get; set; }
    public PrevisionMensuelle PrevisionMensuelle { get; set; } = null!;

    /// <summary>Repris de l'enveloppe, pour filtrer sans jointure.</summary>
    public long ChantierId { get; set; }
    public Chantier Chantier { get; set; } = null!;

    public DateTime Date { get; set; }

    /// <summary>Montant prévu pour cette journée.</summary>
    public decimal MontantPrevu { get; set; }

    /// <summary>Ce qui est prévu de faire ce jour-là, en quelques mots.</summary>
    public string? Observation { get; set; }

    /// <summary>Demandes du chantier rattachées à cette journée.</summary>
    public ICollection<PrevisionJournaliere> Demandes { get; set; } = new List<PrevisionJournaliere>();

    /// <summary>Numéro de la semaine ISO, pour regrouper l'affichage.</summary>
    public int NumeroSemaine =>
        System.Globalization.ISOWeek.GetWeekOfYear(DateTime.SpecifyKind(Date, DateTimeKind.Utc));

    /// <summary>Nom du jour en français, ex : « lundi 11/08 ».</summary>
    public string Libelle => $"{NomDuJour(Date.DayOfWeek)} {Date:dd/MM}";

    public static string NomDuJour(DayOfWeek j) => j switch
    {
        DayOfWeek.Monday => "lundi",
        DayOfWeek.Tuesday => "mardi",
        DayOfWeek.Wednesday => "mercredi",
        DayOfWeek.Thursday => "jeudi",
        DayOfWeek.Friday => "vendredi",
        DayOfWeek.Saturday => "samedi",
        _ => "dimanche"
    };
}
