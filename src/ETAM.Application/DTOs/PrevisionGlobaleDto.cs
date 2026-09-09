namespace ETAM.Application.DTOs;

public class PrevisionGlobaleCreateDto
{
    public long ChantierId { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public string? Observation { get; set; }
    public List<PrevisionGlobaleLigneCreateDto> Lignes { get; set; } = new();
}

public class PrevisionGlobaleLigneCreateDto
{
    public string Rubrique { get; set; } = null!;
    public string Designation { get; set; } = null!;
    public string? Unite { get; set; }
    public decimal Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public string? Observation { get; set; }
}
