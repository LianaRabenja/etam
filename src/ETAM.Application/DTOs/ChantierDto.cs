using System.ComponentModel.DataAnnotations;
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

// Les attributs de validation ci-dessous ne sont pas décoratifs : le projet active
// SuppressImplicitRequiredAttributeForNonNullableReferenceTypes, donc un « string »
// non-nullable n'est PLUS obligatoire tout seul. Sans [Required] explicite, on peut
// enregistrer un chantier sans nom ni code — et il devient introuvable.
public class ChantierCreateDto
{
    [Required(ErrorMessage = "Le nom du chantier est obligatoire.")]
    [StringLength(150, ErrorMessage = "Le nom ne peut pas dépasser 150 caractères.")]
    public string Nom { get; set; } = null!;

    [Required(ErrorMessage = "Le code du chantier est obligatoire.")]
    [StringLength(20, ErrorMessage = "Le code ne peut pas dépasser 20 caractères.")]
    public string Code { get; set; } = null!;

    [StringLength(200)]
    public string? Localisation { get; set; }

    [StringLength(150)]
    public string? Responsable { get; set; }

    [Required(ErrorMessage = "La date de début est obligatoire.")]
    public DateTime DateDebut { get; set; } = DateTime.UtcNow;

    public DateTime? DateFin { get; set; }
    public StatutChantier Statut { get; set; } = StatutChantier.EnPreparation;

    /// <summary>Montant total du marché (ex : 150 000 000).</summary>
    [Range(1, 999999999999, ErrorMessage = "Le montant du marché doit être supérieur à zéro.")]
    public decimal MontantMarche { get; set; }

    /// <summary>Bénéfice conservé (ex : 80 000 000). Le reste va au chantier.</summary>
    [Range(0, 999999999999, ErrorMessage = "Le bénéfice ne peut pas être négatif.")]
    public decimal Benefice { get; set; }

    // --- Compte bancaire créé en même temps que le chantier ---

    /// <summary>Banque choisie dans la liste. Voir le menu Banques pour la gérer.</summary>
    public long? BanqueId { get; set; }

    /// <summary>Nom de la banque (ex : BNI, BOA, BFV...). Rempli depuis la liste déroulante.</summary>
    [Required(ErrorMessage = "Choisissez la banque du chantier.")]
    [StringLength(100)]
    public string NomBanque { get; set; } = "BNI";

    /// <summary>Numéro du compte bancaire du chantier.</summary>
    [StringLength(60)]
    public string? NumeroCompte { get; set; }

    /// <summary>Somme réellement perçue en banque à l'ouverture (peut différer du montant du marché).</summary>
    [Range(0, 999999999999, ErrorMessage = "La somme perçue ne peut pas être négative.")]
    public decimal MontantEnBanque { get; set; }

    [StringLength(1000)]
    public string? Observation { get; set; }
}
