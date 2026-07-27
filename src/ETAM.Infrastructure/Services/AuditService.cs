using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using ETAM.Infrastructure.Persistence;

namespace ETAM.Infrastructure.Services;

/// <summary>Écrit les entrées du journal d'audit.</summary>
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AuditService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task LogAsync(TypeActionAudit action, string? entite = null, string? cleEntite = null,
        string? ancienneValeur = null, string? nouvelleValeur = null, CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            Action = action,
            Entite = entite,
            CleEntite = cleEntite,
            AncienneValeur = ancienneValeur,
            NouvelleValeur = nouvelleValeur,
            UtilisateurId = _currentUser.UserId,
            UtilisateurNom = _currentUser.UserName,
            AdresseIp = _currentUser.IpAddress,
            Navigateur = _currentUser.UserAgent,
            DateAction = DateTime.UtcNow
        };
        await _context.AuditLogs.AddAsync(log, ct);
        await _context.SaveChangesAsync(ct);
    }
}
