using ETAM.Application.DTOs;
using ETAM.Domain.Entities;

namespace ETAM.Application.Common.Mappings;

/// <summary>
/// Conversion entité &lt;-&gt; DTO, écrite à la main.
///
/// Le projet utilisait AutoMapper pour DEUX conversions. Une bibliothèque externe
/// pour si peu, c'est une dépendance à surveiller, à mettre à jour et dont la licence
/// peut changer — ce qui est arrivé. Ici tout est visible, sans magie : si un champ
/// manque, le compilateur le dit, au lieu d'un champ silencieusement vide à l'exécution.
/// </summary>
public static class ChantierMappings
{
    public static ChantierDto ToDto(this Chantier c) => new()
    {
        Id = c.Id,
        Nom = c.Nom,
        Code = c.Code,
        Localisation = c.Localisation,
        Responsable = c.Responsable,
        DateDebut = c.DateDebut,
        DateFin = c.DateFin,
        Statut = c.Statut,
        MontantMarche = c.MontantMarche,
        Benefice = c.Benefice,
        BudgetProjet = c.BudgetProjet,
        BudgetMateriel = c.BudgetMateriel,
        Reserve = c.Reserve,
        ReserveUtilisee = c.ReserveUtilisee,
        Consommation = c.Consommation,
        BudgetMaterielRestant = c.BudgetMaterielRestant,
        ReserveRestante = c.ReserveRestante,
        PourcentageAvancement = c.PourcentageAvancement,
        PourcentageConsomme = c.PourcentageConsomme,
        Observation = c.Observation
    };

    public static List<ChantierDto> ToDto(this IEnumerable<Chantier> chantiers)
        => chantiers.Select(c => c.ToDto()).ToList();

    /// <summary>
    /// Nouveau chantier à partir du formulaire de création.
    /// Les compteurs (consommation, réserve utilisée, matériel transféré) partent
    /// volontairement de zéro : ils sont pilotés par le workflow, jamais saisis.
    /// </summary>
    public static Chantier ToEntity(this ChantierCreateDto dto) => new()
    {
        Nom = dto.Nom,
        Code = dto.Code,
        Localisation = dto.Localisation,
        Responsable = dto.Responsable,
        DateDebut = dto.DateDebut,
        DateFin = dto.DateFin,
        Statut = dto.Statut,
        MontantMarche = dto.MontantMarche,
        Benefice = dto.Benefice,
        Observation = dto.Observation
    };
}
