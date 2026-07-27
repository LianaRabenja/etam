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
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireEstime { get; set; }
    public string? Observation { get; set; }
}
