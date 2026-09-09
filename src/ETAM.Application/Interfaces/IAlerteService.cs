using ETAM.Domain.Entities;
using ETAM.Domain.Enums;

namespace ETAM.Application.Interfaces;

public interface IAlerteService
{
    Task CreerAsync(TypeAlerte type, NiveauAlerte niveau, string titre, string message,
        long? chantierId = null, CancellationToken ct = default);
    Task<IReadOnlyList<Alerte>> ObtenirNonLuesAsync(CancellationToken ct = default);
    /// <summary>Recalcule les alertes budget/stock pour l'ensemble des chantiers.</summary>
    Task EvaluerAlertesAsync(CancellationToken ct = default);
}
