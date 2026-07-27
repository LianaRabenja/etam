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
    /// Exécute la prévision : diminue le Budget Comptes (lignes Compte) ou le Budget Matériel
    /// du chantier + le stock (lignes Materiel). Toutes les opérations sont historisées.
    /// </summary>
    Task<Result> ExecuterAsync(long previsionId, bool utiliserReserve = false, CancellationToken ct = default);
}
