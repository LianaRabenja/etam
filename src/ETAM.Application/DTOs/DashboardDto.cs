namespace ETAM.Application.DTOs;

/// <summary>Agrégats KPI + séries pour le tableau de bord.</summary>
public class DashboardDto
{
    public decimal BudgetCompteRestant { get; set; }
    public decimal BudgetMaterielRestant { get; set; }
    public decimal DepensesDuMois { get; set; }
    public decimal ValeurTotaleMateriaux { get; set; }
    public int NombreChantiers { get; set; }
    public int NombreAlertes { get; set; }
    public int PrevisionsDuJour { get; set; }
    public int ValidationsEnAttente { get; set; }

    public decimal TresorerieTotale { get; set; }
    public decimal TotalDettes { get; set; }

    public double BudgetCompteConsommePct { get; set; }
    public double BudgetMaterielConsommePct { get; set; }

    public List<string> MoisLabels { get; set; } = new();
    public List<decimal> DepensesMensuelles { get; set; } = new();
    public List<string> ChantiersLabels { get; set; } = new();
    public List<decimal> ConsommationParChantier { get; set; } = new();
    public List<string> TopMateriauxLabels { get; set; } = new();
    public List<decimal> TopMateriauxValeurs { get; set; } = new();
}
