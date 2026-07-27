using ETAM.Application.Interfaces;
using ETAM.Domain.Entities;
using ETAM.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ETAM.Infrastructure.Services;

/// <summary>
/// Cache mémoire très court (30 s) pour la liste des chantiers, utilisée comme menu déroulant
/// dans de nombreux formulaires (Approvisionnement, Prévision, Dépenses, Dettes, Matériaux,
/// Rapport de travail, Banques...). Évite de refaire la même requête à chaque affichage de
/// formulaire sans changer aucune logique métier.
/// </summary>
public class ReferenceDataCache : IReferenceDataCache
{
    private const string CleChantiers = "ref:chantiers";
    private static readonly TimeSpan Duree = TimeSpan.FromSeconds(30);

    private readonly IUnitOfWork _uow;
    private readonly IMemoryCache _cache;

    public ReferenceDataCache(IUnitOfWork uow, IMemoryCache cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<IReadOnlyList<Chantier>> ObtenirChantiersAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue<IReadOnlyList<Chantier>>(CleChantiers, out var cached) && cached is not null)
            return cached;

        var chantiers = await _uow.Chantiers.ListAllAsync(ct);
        _cache.Set(CleChantiers, chantiers, Duree);
        return chantiers;
    }

    public void InvaliderChantiers() => _cache.Remove(CleChantiers);
}
