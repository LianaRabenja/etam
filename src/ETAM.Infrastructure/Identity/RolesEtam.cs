namespace ETAM.Infrastructure.Identity;

/// <summary>Rôles applicatifs prédéfinis.</summary>
public static class RolesEtam
{
    public const string Administrateur = "Administrateur";

    /// <summary>
    /// Anciennement « Responsable financier ». Renommé en « Correspondant » à la demande métier.
    /// La VALEUR a changé mais le nom de la constante est conservé pour ne pas casser le code existant.
    /// </summary>
    public const string ResponsableFinancier = "Correspondant";

    public const string ChefDeChantier = "Chef de chantier";

    /// <summary>Rôle limité à la consultation du stock (matériaux) d'un ou plusieurs chantiers.</summary>
    public const string Magasinier = "Magasinier";

    // NB : l'ancien rôle générique "Utilisateur" a été supprimé (il ne servait à rien :
    // aucune autorisation ni menu ne s'appuyait dessus).

    public static readonly string[] Tous =
        { Administrateur, ResponsableFinancier, ChefDeChantier, Magasinier };

    /// <summary>Ancien nom du rôle Correspondant, utilisé pour migrer les bases existantes.</summary>
    public const string AncienNomResponsableFinancier = "Responsable financier";
}
