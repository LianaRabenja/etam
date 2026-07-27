using System.ComponentModel.DataAnnotations;

namespace ETAM.Web.Models;

public class RapportTravailFormViewModel
{
    [Required(ErrorMessage = "Le chantier est obligatoire.")]
    public long ChantierId { get; set; }

    [Required(ErrorMessage = "Le numéro du rapport est obligatoire.")]
    public string Numero { get; set; } = null!;

    [Required] public DateTime PeriodeDebut { get; set; }
    [Required] public DateTime PeriodeFin { get; set; }

    public string? Lieu { get; set; }
    public string? EntrepriseExecutante { get; set; } = "ETAM";
    public string? ConducteurTravaux { get; set; }
    public int EffectifCadres { get; set; }
    public int EffectifOuvriers { get; set; }
    public string? HoraireMatin { get; set; }
    public string? HoraireApresMidi { get; set; }
    public string? ConditionsMeteo { get; set; }

    public string? ResumeSuiviPlanning { get; set; }
    public string? ProblemesRencontres { get; set; }
    public string? Suggestions { get; set; }

    public List<RapportTravailAvancementLigneViewModel> LignesAvancement { get; set; } = new();
    public List<RapportTravailMateriauLigneViewModel> LignesMateriaux { get; set; } = new();
    public List<RapportTravailEquipementLigneViewModel> LignesEquipements { get; set; } = new();
}

public class RapportTravailAvancementLigneViewModel
{
    public string Zone { get; set; } = null!;
    public string TravauxRealises { get; set; } = null!;
    public string NiveauAvancement { get; set; } = null!;
    public string? Observations { get; set; }
}

public class RapportTravailMateriauLigneViewModel
{
    public string Materiau { get; set; } = null!;
    public string? Unite { get; set; }
    public decimal QuantiteUtilisee { get; set; }
    public decimal StockInitial { get; set; }
    public decimal Entree { get; set; }
    public decimal StockRestant { get; set; }
    public string? Observations { get; set; }
}

public class RapportTravailEquipementLigneViewModel
{
    public string Equipement { get; set; } = null!;
    public string? Etat { get; set; }
    public string? Observation { get; set; }
}
