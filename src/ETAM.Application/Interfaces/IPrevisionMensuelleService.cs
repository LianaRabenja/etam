using ETAM.Application.Common.Models;
using ETAM.Domain.Entities;

namespace ETAM.Application.Interfaces;

/// <summary>
/// Enveloppe mensuelle d'un chantier : ouverture du mois, report du reliquat
/// du mois précédent, clôture avec cumul sur le mois suivant.
/// </summary>
public interface IPrevisionMensuelleService
{
    /// <summary>
    /// Ouvre un mois. Le reliquat du mois précédent clôturé est repris
    /// automatiquement dans ReportMoisPrecedent.
    /// </summary>
    Task<Result<long>> CreerAsync(long chantierId, int annee, int mois, decimal montantPrevu,
        IEnumerable<(string Rubrique, string? Designation, decimal Montant, long? PrevisionGlobaleLigneId)>? lignes = null,
        string? observation = null, CancellationToken ct = default);

    /// <summary>Ouvre l'enveloppe : les prévisions journalières peuvent s'y imputer.</summary>
    Task<Result> ValiderAsync(long id, CancellationToken ct = default);

    Task<Result> RefuserAsync(long id, string motif, CancellationToken ct = default);

    /// <summary>
    /// Clôture le mois et reporte le reliquat non décaissé sur le mois suivant.
    /// Refusée tant qu'une prévision journalière du mois reste ouverte.
    /// </summary>
    Task<Result<decimal>> CloturerAsync(long id, CancellationToken ct = default);

    /// <summary>Enveloppe ouverte du chantier couvrant cette date, s'il en existe une.</summary>
    Task<PrevisionMensuelle?> ObtenirMoisOuvertAsync(long chantierId, DateTime date, CancellationToken ct = default);

    /// <summary>Total déjà engagé sur les mois du chantier, pour contrôler le budget projet.</summary>
    Task<decimal> TotalMoisEngagesAsync(long chantierId, long? saufId = null, CancellationToken ct = default);
}
