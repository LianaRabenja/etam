using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Rapport de travail (rapport hebdomadaire d'avancement des travaux) d'un chantier.
/// Reprend les mêmes libellés que le rapport papier historique (n°, période, effectifs,
/// horaires, météo, avancement par ouvrage, suivi matériaux, matériels, problèmes, suggestions).
/// Créé par le Correspondant, il doit être validé par l'Administrateur avant d'être définitif.
/// </summary>
public class RapportTravail : BaseEntity
{
    public long ChantierId { get; set; }
    public Chantier Chantier { get; set; } = null!;

    /// <summary>Numéro du rapport (ex : "06"). Libre, choisi par le Correspondant.</summary>
    public string Numero { get; set; } = null!;

    public DateTime PeriodeDebut { get; set; }
    public DateTime PeriodeFin { get; set; }

    // --- 1. Informations générales ---
    public string? Lieu { get; set; }
    public string? EntrepriseExecutante { get; set; }
    public string? ConducteurTravaux { get; set; }
    public int EffectifCadres { get; set; }
    public int EffectifOuvriers { get; set; }
    public string? HoraireMatin { get; set; }
    public string? HoraireApresMidi { get; set; }
    public string? ConditionsMeteo { get; set; }

    // --- 3. Résumé des travaux effectués suivant planning ---
    public string? ResumeSuiviPlanning { get; set; }

    // --- 6. Problèmes rencontrés ---
    public string? ProblemesRencontres { get; set; }

    // --- 7. Suggestions ---
    public string? Suggestions { get; set; }

    // --- Workflow ---
    public StatutRapportTravail Statut { get; set; } = StatutRapportTravail.Brouillon;
    public string? SoumisPar { get; set; }
    public DateTime? DateSoumission { get; set; }
    public string? ValideParId { get; set; }
    public DateTime? DateValidation { get; set; }
    public string? MotifRefus { get; set; }

    // --- Navigation ---
    public ICollection<RapportTravailAvancementLigne> LignesAvancement { get; set; } = new List<RapportTravailAvancementLigne>();
    public ICollection<RapportTravailMateriauLigne> LignesMateriaux { get; set; } = new List<RapportTravailMateriauLigne>();
    public ICollection<RapportTravailEquipementLigne> LignesEquipements { get; set; } = new List<RapportTravailEquipementLigne>();

    // --- Propriétés calculées ---
    public bool EstModifiable =>
        Statut == StatutRapportTravail.Brouillon || Statut == StatutRapportTravail.Refuse;
}

/// <summary>Ligne « 2. Avancement des travaux » : Travaux réalisés / Niveau d'avancement / Observations.</summary>
public class RapportTravailAvancementLigne : BaseEntity
{
    public long RapportTravailId { get; set; }
    public RapportTravail RapportTravail { get; set; } = null!;

    /// <summary>Zone ou ouvrage concerné (ex : "Centre de formation", "Église - Poteaux Pt A").</summary>
    public string Zone { get; set; } = null!;
    public string TravauxRealises { get; set; } = null!;
    public string NiveauAvancement { get; set; } = null!;
    public string? Observations { get; set; }
}

/// <summary>Ligne « 4. Suivi des matériaux » : matériaux utilisés / état du stock.</summary>
public class RapportTravailMateriauLigne : BaseEntity
{
    public long RapportTravailId { get; set; }
    public RapportTravail RapportTravail { get; set; } = null!;

    public string Materiau { get; set; } = null!;
    public string? Unite { get; set; }
    public decimal QuantiteUtilisee { get; set; }
    public decimal StockInitial { get; set; }
    public decimal Entree { get; set; }
    public decimal StockRestant { get; set; }
    public string? Observations { get; set; }
}

/// <summary>Ligne « 5. Matériels et équipements » : équipement / état / observation.</summary>
public class RapportTravailEquipementLigne : BaseEntity
{
    public long RapportTravailId { get; set; }
    public RapportTravail RapportTravail { get; set; } = null!;

    public string Equipement { get; set; } = null!;
    public string? Etat { get; set; }
    public string? Observation { get; set; }
}
