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
    public long ChantierId { get; set; }
    public DateTime DatePrevision { get; set; } = DateTime.UtcNow;
    public string? Observation { get; set; }
    public List<PrevisionLigneCreateDto> Lignes { get; set; } = new();
}

public class PrevisionLigneCreateDto
{
    public string Designation { get; set; } = null!;
    public string Categorie { get; set; } = null!;
    public TypeBudget TypeBudget { get; set; }
    public long? MateriauId { get; set; }
    public long? DetteFournisseurId { get; set; }

    /// <summary>Poste de la prévision globale sur lequel cette dépense est imputée.</summary>
    public long? PrevisionGlobaleLigneId { get; set; }

    public decimal Quantite { get; set; }
    public decimal PrixUnitaireEstime { get; set; }
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
