using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Journal d'audit : trace connexions, CRUD, validations avec ancienne/nouvelle valeur,
/// utilisateur, IP et navigateur.
/// </summary>
public class AuditLog : BaseEntity
{
    public TypeActionAudit Action { get; set; }
    public string? Entite { get; set; }
    public string? CleEntite { get; set; }

    public string? UtilisateurId { get; set; }
    public string? UtilisateurNom { get; set; }

    public string? AdresseIp { get; set; }
    public string? Navigateur { get; set; }

    public string? AncienneValeur { get; set; }
    public string? NouvelleValeur { get; set; }

    public DateTime DateAction { get; set; } = DateTime.UtcNow;
}
