using System.ComponentModel.DataAnnotations;

namespace ETAM.Web.Models;

public class CreateUserViewModel
{
    [Required, EmailAddress]
    [Display(Name = "Adresse email")]
    public string Email { get; set; } = null!;

    [Required]
    [Display(Name = "Nom complet")]
    public string NomComplet { get; set; } = null!;

    [Display(Name = "Fonction")]
    public string? Fonction { get; set; }

    [Required, DataType(DataType.Password), MinLength(8)]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = null!;

    [Required]
    [Display(Name = "Rôle")]
    public string Role { get; set; } = null!;
}
