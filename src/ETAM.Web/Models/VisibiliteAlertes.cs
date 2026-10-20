using System.Security.Claims;
using ETAM.Domain.Enums;

namespace ETAM.Web.Models;

/// <summary>
/// Qui voit quelles notifications.
///
/// Les alertes sont stockées sans destinataire : une seule ligne en base, lue par
/// tout le monde. Le tri se fait donc à la lecture, en fonction du rôle.
///
///   Administrateur       — tout. C'est lui qui pilote l'argent, le stock et les seuils.
///   Responsable Financier — ce qu'il doit valider, et le sort des prévisions.
///   Chef de chantier      — uniquement le sort de ses prévisions : validée ou refusée.
///
/// Sans ce tri, le Chef de chantier recevait les alertes de budget dépassé, de stock
/// critique et de seuil à 50 % : du bruit qu'il ne peut pas traiter et qui noie la
/// seule information qui le concerne.
/// </summary>
public static class VisibiliteAlertes
{
    /// <summary>Ce que voit la Finance : ses validations en attente, et les décisions.</summary>
    private static readonly TypeAlerte[] PourFinance =
    {
        TypeAlerte.ValidationEnAttente,
        TypeAlerte.PrevisionEnAttente,
        TypeAlerte.DecisionPrevision
    };

    /// <summary>Ce que voit le Chef de chantier : le sort de ses prévisions, rien d'autre.</summary>
    private static readonly TypeAlerte[] PourChef =
    {
        TypeAlerte.DecisionPrevision
    };

    /// <summary>
    /// Types d'alertes visibles par cet utilisateur, ou <c>null</c> s'il les voit toutes.
    /// L'Administrateur passe en premier : s'il cumule plusieurs rôles, il garde le sien.
    /// </summary>
    public static TypeAlerte[]? TypesVisibles(ClaimsPrincipal user)
    {
        if (user.IsInRole("Administrateur")) return null;
        if (user.IsInRole("Correspondant")) return PourFinance;
        if (user.IsInRole("Chef de chantier")) return PourChef;

        // Rôle non prévu (Magasinier par exemple) : rien plutôt que tout.
        return Array.Empty<TypeAlerte>();
    }
}
