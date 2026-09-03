using ETAM.Application.DTOs;
using FluentValidation;

namespace ETAM.Application.Validators;

public class ChantierCreateValidator : AbstractValidator<ChantierCreateDto>
{
    public ChantierCreateValidator()
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.BudgetMateriel).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Reserve).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DateFin)
            .GreaterThanOrEqualTo(x => x.DateDebut)
            .When(x => x.DateFin.HasValue)
            .WithMessage("La date de fin doit être postérieure à la date de début.");
    }
}
