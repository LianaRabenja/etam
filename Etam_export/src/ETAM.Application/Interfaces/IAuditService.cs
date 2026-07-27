using ETAM.Domain.Enums;

namespace ETAM.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(TypeActionAudit action, string? entite = null, string? cleEntite = null,
        string? ancienneValeur = null, string? nouvelleValeur = null, CancellationToken ct = default);
}
