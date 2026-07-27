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

    /// <summary>
    /// Chantier d'affectation. Pour un Magasinier (ou un Chef de chantier), il ne voit
    /// que le stock et les fiches de CE chantier. Null = accès à tous les chantiers.
    /// </summary>
    public long? ChantierId { get; set; }
}
