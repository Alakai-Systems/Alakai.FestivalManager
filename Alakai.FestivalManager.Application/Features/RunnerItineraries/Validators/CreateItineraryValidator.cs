namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Validators;

public class CreateItineraryValidator : AbstractValidator<CreateItineraryCommand>
{
    public CreateItineraryValidator()
    {
        RuleFor(x => x.EditionId).NotEmpty();
        RuleFor(x => x.DateTime).NotEmpty();
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Direction).IsInEnum();
        RuleFor(x => x.RunnerName).MaximumLength(150);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}