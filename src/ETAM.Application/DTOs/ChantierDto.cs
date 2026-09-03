using ETAM.Domain.Enums;

namespace ETAM.Application.DTOs;

public class ChantierDto
{
    public long Id { get; set; }
    public string Nom { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Localisation { get; set; }
    public string? Responsable { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public StatutChantier Statut { get; set; }
    public decimal MontantMarche { get; set; }
    public decimal Benefice { get; set; }
    public decimal BudgetProjet { get; set; }
    public decimal BudgetMateriel { get; set; }
    public decimal Reserve { get; set; }
    public decimal ReserveUtilisee { get; set; }
    public decimal Consommation { get; set; }
    public decimal BudgetMaterielRestant { get; set; }
    public decimal ReserveRestante { get; set; }
    public double PourcentageAvancement { get; set; }
    public double PourcentageConsomme { get; set; }
    public string? Observation { get; set; }
}

public class ChantierCreateDto
{
    public string Nom { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Localisation { get; set; }
    public string? Responsable { get; set; }
    public DateTime DateDebut { get; set; } = DateTime.UtcNow;
    public DateTime? DateFin { get; set; }
    public StatutChantier Statut { get; set; } = StatutChantier.EnPreparation;
    /// <summary>Montant total du marché (ex : 150 000 000).</summary>
    public decimal MontantMarche { get; set; }
    /// <summary>Bénéfice conservé (ex : 80 000 000). Le reste va au chantier.</summary>
    public decimal Benefice { get; set; }
    public decimal BudgetMateriel { get; set; }
    public decimal Reserve { get; set; }
    public string? Observation { get; set; }
}
