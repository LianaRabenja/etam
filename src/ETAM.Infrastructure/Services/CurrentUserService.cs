using System.Security.Claims;
using ETAM.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ETAM.Infrastructure.Services;

/// <summary>Récupère l'identité et le contexte HTTP de l'utilisateur courant.</summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    public string? UserId => _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? UserName => _accessor.HttpContext?.User?.Identity?.Name;
    public string? IpAddress => _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    public string? UserAgent => _accessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

    public bool IsInRole(string role) => _accessor.HttpContext?.User?.IsInRole(role) ?? false;
}
