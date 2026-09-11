using ETAM.Application.DTOs;
using FluentValidation;

namespace ETAM.Application.Validators;

public class ChantierCreateValidator : AbstractValidator<ChantierCreateDto>
{
    public ChantierCreateValidator()
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.NomBanque).NotEmpty().MaximumLength(100)
            .WithMessage("Indiquez le nom de la banque du chantier.");
        RuleFor(x => x.NumeroCompte).MaximumLength(50);
        RuleFor(x => x.MontantEnBanque).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DateFin)
            .GreaterThanOrEqualTo(x => x.DateDebut)
            .When(x => x.DateFin.HasValue)
            .WithMessage("La date de fin doit être postérieure à la date de début.");
    }
}
