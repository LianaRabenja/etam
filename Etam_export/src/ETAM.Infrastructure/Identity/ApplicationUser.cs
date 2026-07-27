using Microsoft.AspNetCore.Identity;

namespace ETAM.Infrastructure.Identity;

/// <summary>Utilisateur applicatif ETAM (étend l'identité ASP.NET).</summary>
public class ApplicationUser : IdentityUser
{
    public string? NomComplet { get; set; }
    public string? Fonction { get; set; }
    public bool EstActif { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DerniereConnexion { get; set; }
}
