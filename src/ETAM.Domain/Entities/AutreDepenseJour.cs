using ETAM.Domain.Common;

namespace ETAM.Domain.Entities;

/// <summary>
/// Une ligne du récapitulatif du jour qui ne vient pas d'un chantier.
///
/// Sur la feuille papier, à côté des chantiers, on trouve toutes les autres sorties
/// de la journée : DAF CHAUFFEUR, CARTE ROSE, AVANCE COMPRESSEUR, chambre majunga,
/// biriky cochon... Ce sont des dépenses réelles qu'il faut sortir de la banque le
/// même jour, mais qui n'appartiennent à aucun chantier.
///
/// Elles se saisissent directement sur l'écran du récapitulatif et s'additionnent
/// au total de la journée.
/// </summary>
public class AutreDepenseJour : BaseEntity
{
    /// <summary>Journée concernée.</summary>
    public DateTime Date { get; set; }

    /// <summary>Ce qui est payé, tel qu'il apparaîtra sur la feuille.</summary>
    public string Libelle { get; set; } = null!;

    public decimal Montant { get; set; }

    /// <summary>Position dans la feuille, pour retrouver l'ordre habituel.</summary>
    public int Ordre { get; set; }

    /// <summary>Chantier concerné quand la dépense en vise un sans passer par une prévision.</summary>
    public long? ChantierId { get; set; }
    public Chantier? Chantier { get; set; }

    public string? Observation { get; set; }
}
