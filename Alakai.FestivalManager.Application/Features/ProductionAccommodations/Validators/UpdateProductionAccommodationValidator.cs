namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Validators;

public class UpdateProductionAccommodationValidator : AbstractValidator<UpdateProductionAccommodationCommand>
{
    public UpdateProductionAccommodationValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ProductionAccommodationZoneId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Capacity).GreaterThan(0);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}