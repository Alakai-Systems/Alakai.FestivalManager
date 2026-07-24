namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Validators;

public class CreateReservationValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.EditionId).NotEmpty();
        RuleFor(x => x.ProductionAccommodationBuildingId).NotEmpty();
        RuleForEach(x => x.Occupants).ChildRules(o =>
        {
            o.RuleFor(x => x.ProductionPersonId).NotEmpty();
        });
    }
}