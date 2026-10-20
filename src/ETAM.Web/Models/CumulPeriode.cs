namespace ETAM.Web.Models;

/// <summary>
/// Cumul de la SEMAINE EN COURS d'un chantier.
///
/// L'argent quitte la banque à l'exécution d'une prévision, et il est dépensé à ce
/// moment-là : exécuté = dépensé. Le cumul, c'est donc la somme des journées
/// exécutées de la semaine, confrontée à la PRÉVISION DE LA SEMAINE — c'est elle qui
/// tient le rôle de l'argent réel de la semaine.
///
/// <see cref="Reste"/> = prévision de la semaine − dépensé. Cet argent est RESTÉ EN
/// BANQUE : il n'est jamais sorti, il n'y a donc rien à y remettre à la clôture.
/// Un reste négatif est un dépassement de la prévision.
/// </summary>
public class CumulPeriode
{
    public long ChantierId { get; init; }
    public string ChantierNom { get; init; } = "";

    /// <summary>Nombre de journées exécutées et non clôturées dans la semaine.</summary>
    public int NbJournees { get; init; }

    /// <summary>Lundi de la semaine en cours.</summary>
    public DateTime? Debut { get; init; }

    /// <summary>Total de la prévision de la semaine : l'argent réel de la semaine.</summary>
    public decimal PlanSemaine { get; init; }

    /// <summary>Sorti de la banque dans la semaine — et donc dépensé.</summary>
    public decimal Sorti { get; init; }

    /// <summary>Même argent que <see cref="Sorti"/> : l'exécution EST la dépense.</summary>
    public decimal Depense { get; init; }

    /// <summary>Prévision de la semaine moins dépensé. Négatif = dépassement.</summary>
    public decimal Reste { get; init; }

    /// <summary>Conservé pour l'affichage de l'historique.</summary>
    public decimal? ResteDerniereJournee { get; init; }

    public DateTime? DerniereCloture { get; init; }
    public decimal? MontantDerniereCloture { get; init; }

    /// <summary>La prévision de la semaine est dépassée.</summary>
    public bool Depassement => Reste < 0;

    /// <summary>Part de la prévision de la semaine déjà consommée.</summary>
    public double PourcentagePlan =>
        PlanSemaine <= 0 ? 0 : (double)(Sorti / PlanSemaine) * 100;

    /// <summary>
    /// Ancien nom, gardé pour les vues : il mesure désormais la consommation
    /// de la prévision de la semaine, pas le rapport entre deux colonnes identiques.
    /// </summary>
    public double PourcentageDepense => PourcentagePlan;

    /// <summary>Il y a au moins une journée exécutée : la semaine peut être clôturée.</summary>
    public bool PeutCloturer => NbJournees > 0;

    /// <summary>
    /// Aucune prévision n'a été saisie pour cette semaine : rien à quoi comparer
    /// les dépenses. C'est à signaler, pas à masquer.
    /// </summary>
    public bool SansPlan => NbJournees > 0 && PlanSemaine <= 0;

    /// <summary>
    /// Le contrôle de cohérence de l'ancienne chaîne de reports n'a plus d'objet :
    /// il n'y a plus de report d'une journée sur l'autre.
    /// </summary>
    public bool Incoherent => false;
}
