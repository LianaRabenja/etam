using ETAM.Application.Common.Models;
using ETAM.Application.DTOs;

namespace ETAM.Application.Interfaces;

/// <summary>Service du module principal : workflow des prévisions journalières.</summary>
public interface IPrevisionService
{
    Task<Result<long>> CreerAsync(PrevisionCreateDto dto, CancellationToken ct = default);
    Task<Result> SoumettreAsync(long previsionId, CancellationToken ct = default);
    Task<Result> ValiderResponsableFinancierAsync(long previsionId, CancellationToken ct = default);
    Task<Result> ValiderAdministrateurAsync(long previsionId, CancellationToken ct = default);
    Task<Result> RefuserAsync(long previsionId, string motif, CancellationToken ct = default);

    /// <summary>
    /// Ouvre l'enveloppe de la journée : rattache la prévision au mois en cours et y
    /// ajoute le reliquat non décaissé de la journée précédente.
    ///
    /// Ne sort aucun argent de la banque et n'impute aucun budget : c'est le rôle
    /// des décaissements (<see cref="IDecaissementService"/>).
    /// </summary>
    Task<Result> ExecuterAsync(long previsionId, bool utiliserReserve = false, CancellationToken ct = default);

    /// <summary>
    /// Le chef de chantier atteste avoir reçu l'argent mis à disposition.
    /// Tant que ce n'est pas signé, aucun décaissement n'est autorisé.
    /// </summary>
    Task<Result> AccuserReceptionAsync(long previsionId, string nomSignataire, CancellationToken ct = default);
}
