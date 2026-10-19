using ETAM.Domain.Common;

namespace ETAM.Domain.Entities;

/// <summary>
/// Une banque de la place : BNI, BOA, BFV-SG, BMOI...
///
/// Sert de liste de référence à la création d'un chantier : on choisit la banque
/// au lieu de la retaper, pour éviter d'avoir « BNI », « B.N.I » et « bni » dans
/// la même base et de fausser les regroupements.
///
/// Le compte bancaire garde le NOM de la banque en texte, pas un lien vers cette
/// table : renommer ou désactiver une banque ne doit jamais toucher l'historique
/// des comptes déjà ouverts.
/// </summary>
public class Banque : BaseEntity
{
    /// <summary>Nom complet, ex : « Bank of Africa ».</summary>
    public string Nom { get; set; } = null!;

    /// <summary>Sigle usuel, ex : « BOA ». C'est lui qui s'affiche sur les comptes.</summary>
    public string Sigle { get; set; } = null!;

    /// <summary>Agence ou localisation habituelle, facultatif.</summary>
    public string? Agence { get; set; }

    /// <summary>
    /// Une banque désactivée n'est plus proposée à la création d'un chantier,
    /// mais reste visible dans la liste et n'affecte pas les comptes existants.
    /// </summary>
    public bool EstActive { get; set; } = true;

    public string? Observation { get; set; }

    /// <summary>Libellé affiché dans les listes déroulantes.</summary>
    public string Libelle => string.IsNullOrWhiteSpace(Agence)
        ? $"{Sigle} — {Nom}"
        : $"{Sigle} — {Nom} ({Agence})";
}
