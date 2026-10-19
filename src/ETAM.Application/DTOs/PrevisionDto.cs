using System.ComponentModel.DataAnnotations;
using ETAM.Domain.Enums;

namespace ETAM.Application.DTOs;

public class PrevisionDto
{
    public long Id { get; set; }
    public long ChantierId { get; set; }
    public string? ChantierNom { get; set; }
    public DateTime DatePrevision { get; set; }
    public string Reference { get; set; } = null!;
    public StatutPrevision Statut { get; set; }
    public decimal Total { get; set; }
    public string? Observation { get; set; }
    public List<PrevisionLigneDto> Lignes { get; set; } = new();
}

public class PrevisionLigneDto
{
    public long Id { get; set; }
    public string Designation { get; set; } = null!;
    public string Categorie { get; set; } = null!;
    public TypeBudget TypeBudget { get; set; }
    public long? MateriauId { get; set; }
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireEstime { get; set; }
    public decimal Total { get; set; }
    public string? Observation { get; set; }
}

public class PrevisionCreateDto
{
    [Range(1, long.MaxValue, ErrorMessage = "Choisissez le chantier.")]
    public long ChantierId { get; set; }

    [Required(ErrorMessage = "La date de la journée est obligatoire.")]
    public DateTime DatePrevision { get; set; } = DateTime.UtcNow;

    [StringLength(1000)]
    public string? Observation { get; set; }

    public List<PrevisionLigneCreateDto> Lignes { get; set; } = new();
}

// Comme pour le chantier : sans [Required] explicite, une ligne sans désignation
// passe la validation et se retrouve en base avec un libellé vide, impossible à
// relire plus tard. Les montants, eux, sont contrôlés dans le contrôleur — APRÈS
// que le prix du catalogue a été appliqué, sinon une ligne qui attend son prix
// du catalogue serait refusée avant même de l'avoir reçu.
public class PrevisionLigneCreateDto
{
    [Required(ErrorMessage = "Chaque ligne doit avoir une désignation.")]
    [StringLength(150)]
    public string Designation { get; set; } = null!;

    [Required(ErrorMessage = "Chaque ligne doit avoir une catégorie.")]
    [StringLength(80)]
    public string Categorie { get; set; } = null!;

    public TypeBudget TypeBudget { get; set; }
    public long? MateriauId { get; set; }
    public long? DetteFournisseurId { get; set; }

    /// <summary>Poste de la prévision globale sur lequel cette dépense est imputée.</summary>
    public long? PrevisionGlobaleLigneId { get; set; }

    public decimal Quantite { get; set; }
    public decimal PrixUnitaireEstime { get; set; }

    [StringLength(500)]
    public string? Observation { get; set; }
}

/// <summary>
/// Poste de la prévision globale proposé à la saisie : enveloppe prévue,
/// montant déjà consommé et reste disponible.
/// </summary>
public class PosteGlobalDto
{
    public long Id { get; set; }
    public long ChantierId { get; set; }
    public string Rubrique { get; set; } = null!;
    public string Designation { get; set; } = null!;
    public decimal Enveloppe { get; set; }
    public decimal Consomme { get; set; }
    public decimal Reste => Enveloppe - Consomme;
}
