namespace ETAM.Web.Models;

/// <summary>
/// Cumul de la période en cours d'un chantier : ce qui est sorti de la banque
/// depuis la dernière clôture, ce qui a été dépensé, et ce qu'il reste à utiliser.
///
/// Le reste n'est PAS la somme des restes journaliers : celui d'une journée est
/// déjà compté dans le plafond de la suivante. Il vaut « sorti moins dépensé »,
/// et doit retomber sur le reste de la dernière journée — sinon la chaîne des
/// reports est rompue, et <see cref="Incoherent"/> le signale.
/// </summary>
public class CumulPeriode
{
    public long ChantierId { get; init; }
    public string ChantierNom { get; init; } = "";

    /// <summary>Nombre de journées ouvertes depuis la dernière clôture.</summary>
    public int NbJournees { get; init; }

    /// <summary>Première journée de la période en cours.</summary>
    public DateTime? Debut { get; init; }

    public decimal Sorti { get; init; }
    public decimal Depense { get; init; }
    public decimal Reste { get; init; }

    /// <summary>Reste porté par la dernière journée, pour le contrôle de cohérence.</summary>
    public decimal? ResteDerniereJournee { get; init; }

    public DateTime? DerniereCloture { get; init; }
    public decimal? MontantDerniereCloture { get; init; }

    public double PourcentageDepense => Sorti <= 0 ? 0 : (double)(Depense / Sorti) * 100;

    /// <summary>Il y a au moins une journée ouverte : la période peut être clôturée.</summary>
    public bool PeutCloturer => NbJournees > 0;

    /// <summary>
    /// Les deux calculs du reste ne concordent pas : une journée a probablement été
    /// supprimée ou un report n'a pas été repris. À vérifier avant de clôturer.
    /// </summary>
    public bool Incoherent =>
        ResteDerniereJournee.HasValue && Math.Abs(ResteDerniereJournee.Value - Reste) > 1m;
}
