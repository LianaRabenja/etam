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
    /// Exécute la prévision de la journée : l'argent SORT de la banque et il est
    /// dépensé. L'enveloppe du mois se consomme d'autant, et la semaine se compare
    /// aussitôt à son plan.
    ///
    /// Il n'y a plus de report d'une journée sur l'autre : rien ne reste dehors.
    /// </summary>
    Task<Result> ExecuterAsync(long previsionId, bool utiliserReserve = false, CancellationToken ct = default);

    /// <summary>
    /// Le chef de chantier atteste avoir reçu l'argent mis à disposition.
    /// Tant que ce n'est pas signé, aucun décaissement n'est autorisé.
    /// </summary>
    Task<Result> AccuserReceptionAsync(long previsionId, string nomSignataire, CancellationToken ct = default);

    /// <summary>
    /// Clôture la semaine en cours d'un chantier : on arrête les comptes de la semaine
    /// et on constate l'écart avec son plan. AUCUN mouvement bancaire n'est fait —
    /// l'argent non dépensé n'a jamais quitté le compte.
    ///
    /// Retourne la part du plan de la semaine qui n'a pas été dépensée (négatif en
    /// cas de dépassement). C'est une information, pas un transfert.
    /// </summary>
    Task<Result<decimal>> CloturerPeriodeAsync(long chantierId, CancellationToken ct = default);
}
