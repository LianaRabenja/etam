using ETAM.Domain.Enums;

namespace ETAM.Application.DTOs;

public class ApprovisionnementCreateDto
{
    public long ChantierId { get; set; }
    public DateTime DateAppro { get; set; } = DateTime.UtcNow;
    public string? Observation { get; set; }
    public List<ApprovisionnementLigneCreateDto> Lignes { get; set; } = new();
}

public class ApprovisionnementLigneCreateDto
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
