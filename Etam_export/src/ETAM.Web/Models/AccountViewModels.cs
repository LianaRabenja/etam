using System.ComponentModel.DataAnnotations;

namespace ETAM.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "L'email est requis.")]
    [EmailAddress]
    [Display(Name = "Adresse email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Le mot de passe est requis.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = null!;

    [Display(Name = "Se souvenir de moi")]
    public bool RememberMe { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Adresse email")]
    public string Email { get; set; } = null!;
}

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Token { get; set; } = null!;

    [Required(ErrorMessage = "Le mot de passe est requis.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nouveau mot de passe")]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmer le mot de passe")]
    [Compare(nameof(Password), ErrorMessage = "Les mots de passe ne correspondent pas.")]
    public string ConfirmPassword { get; set; } = null!;
}
