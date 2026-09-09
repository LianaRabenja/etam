using ETAM.Application.DTOs;
using FluentValidation;

namespace ETAM.Application.Validators;

public class PrevisionCreateValidator : AbstractValidator<PrevisionCreateDto>
{
    public PrevisionCreateValidator()
    {
        RuleFor(x => x.ChantierId).GreaterThan(0);
        RuleFor(x => x.Lignes).NotEmpty().WithMessage("Une prévision doit contenir au moins une ligne.");
        RuleForEach(x => x.Lignes).ChildRules(l =>
        {
            l.RuleFor(x => x.Designation).NotEmpty();
            l.RuleFor(x => x.Quantite).GreaterThan(0);
            l.RuleFor(x => x.PrixUnitaireEstime).GreaterThanOrEqualTo(0);
        });
    }
}
