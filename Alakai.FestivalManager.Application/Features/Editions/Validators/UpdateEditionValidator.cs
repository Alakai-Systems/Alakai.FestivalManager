namespace Alakai.FestivalManager.Application.Features.Editions.Validators;

public class UpdateEditionValidator : AbstractValidator<UpdateEditionCommand>
{
    public UpdateEditionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.FestivalId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100);

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate);

        RuleFor(x => x.RegistrationOpenDate)
            .LessThan(x => x.RegistrationCloseDate)
            .When(x => x.RegistrationOpenDate.HasValue &&
                       x.RegistrationCloseDate.HasValue);
    }
}