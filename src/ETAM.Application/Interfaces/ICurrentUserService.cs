namespace ETAM.Application.Interfaces;

/// <summary>Fournit l'identité de l'utilisateur courant et le contexte HTTP (IP, navigateur).</summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    bool IsInRole(string role);
}
