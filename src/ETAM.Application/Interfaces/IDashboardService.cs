using ETAM.Application.DTOs;

namespace ETAM.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> ObtenirAsync(CancellationToken ct = default);
}
