using ETAM.Domain.Common;

namespace ETAM.Domain.Entities;

/// <summary>
/// Répartition facultative de l'enveloppe mensuelle par rubrique
/// (ex : Approvisionnement 80 000 000, Main d'œuvre 30 000 000, Transport 15 000 000).
/// Sert à comparer le prévu du mois au réellement décaissé, poste par poste.
/// </summary>
public class PrevisionMensuelleLigne : BaseEntity
{
    public long PrevisionMensuelleId { get; set; }
    public PrevisionMensuelle PrevisionMensuelle { get; set; } = null!;

    /// <summary>Rubrique, reprise du vocabulaire de la prévision globale.</summary>
    public string Rubrique { get; set; } = null!;

    public string? Designation { get; set; }

    /// <summary>Montant alloué à cette rubrique pour le mois.</summary>
    public decimal Montant { get; set; }

    /// <summary>Poste du plan projet auquel cette ligne se rattache, s'il existe.</summary>
    public long? PrevisionGlobaleLigneId { get; set; }
    public PrevisionGlobaleLigne? PrevisionGlobaleLigne { get; set; }

    public string? Observation { get; set; }
}
