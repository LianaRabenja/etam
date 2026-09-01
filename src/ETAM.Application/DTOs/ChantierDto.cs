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
    // Le Budget Matériel n'est plus saisi : il vaut toujours marché − bénéfice,
    // et c'est le contrôleur qui le calcule à l'enregistrement.
    public decimal Reserve { get; set; }
    public string? Observation { get; set; }

    // --- Compte bancaire du chantier (créé en même temps que le chantier) ---
    /// <summary>Nom de la banque (ex : BGFI Madagascar). Obligatoire à la saisie.</summary>
    public string? Banque { get; set; }
    /// <summary>Numéro du compte (ex : 41000869011-66).</summary>
    public string? NumeroCompte { get; set; }
    /// <summary>Intitulé du compte (ex : SARL ETAM). Par défaut « Compte {nom du chantier} ».</summary>
    public string? NomCompte { get; set; }
    /// <summary>
    /// Argent RÉELLEMENT encaissé à ce jour (avance, premier décompte...).
    /// Surtout pas le montant du marché : il n'est encaissé qu'au fil des décomptes.
    /// </summary>
    public decimal MontantEncaisse { get; set; }
    /// <summary>Libellé de ce premier encaissement (ex : « Avance de démarrage 20 % »).</summary>
    public string? MotifEncaissement { get; set; }
}
