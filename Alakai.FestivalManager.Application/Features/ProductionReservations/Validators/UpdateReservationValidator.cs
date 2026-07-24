namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Validators;

public class UpdateReservationValidator : AbstractValidator<UpdateReservationCommand>
{
    public UpdateReservationValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleForEach(x => x.Occupants).ChildRules(o =>
        {
            o.RuleFor(x => x.ProductionPersonId).NotEmpty();
        });
    }
}