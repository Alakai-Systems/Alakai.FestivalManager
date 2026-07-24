namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Validators;

public class CreateTripValidator : AbstractValidator<CreateTripCommand>
{
    public CreateTripValidator()
    {
        RuleFor(x => x.EditionId).NotEmpty();
        RuleFor(x => x.ProductionPersonId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.TripNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DateTime).NotEmpty();
        RuleFor(x => x.TerminalOrStation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Direction).IsInEnum();
    }
}