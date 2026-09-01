using ETAM.Application.DTOs;
using FluentValidation;

namespace ETAM.Application.Validators;

public class ChantierCreateValidator : AbstractValidator<ChantierCreateDto>
{
    public ChantierCreateValidator()
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        // Le Budget Matériel n'est plus saisi : il est calculé (marché − bénéfice).
        RuleFor(x => x.Reserve).GreaterThanOrEqualTo(0);

        RuleFor(x => x.MontantMarche).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Benefice)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(x => x.MontantMarche)
            .WithMessage("Le bénéfice ne peut pas dépasser le montant du marché : " +
                         "le budget du projet deviendrait négatif.");

        // --- Compte bancaire créé avec le chantier ---
        RuleFor(x => x.MontantEncaisse)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(x => x.MontantMarche)
            .WithMessage("L'argent encaissé ne peut pas dépasser le montant du marché.");
        RuleFor(x => x.Banque).MaximumLength(120);
        RuleFor(x => x.NomCompte).MaximumLength(150);
        RuleFor(x => x.NumeroCompte).MaximumLength(60);

        RuleFor(x => x.DateFin)
            .GreaterThanOrEqualTo(x => x.DateDebut)
            .When(x => x.DateFin.HasValue)
            .WithMessage("La date de fin doit être postérieure à la date de début.");
    }
}
