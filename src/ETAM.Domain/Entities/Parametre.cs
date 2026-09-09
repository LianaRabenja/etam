using ETAM.Domain.Common;

namespace ETAM.Domain.Entities;

/// <summary>
/// Paramètres généraux de l'application (clé/valeur) : logo, nom entreprise,
/// devise, exercice comptable, seuils d'alerte, SMTP, sécurité...
/// </summary>
public class Parametre : BaseEntity
{
    public string Cle { get; set; } = null!;
    public string? Valeur { get; set; }
    public string? Groupe { get; set; }
    public string? Description { get; set; }
}
