namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Validators;

public class CreateProductionAccommodationZoneValidator : AbstractValidator<CreateProductionAccommodationZoneCommand>
{
    public CreateProductionAccommodationZoneValidator()
    {
        RuleFor(x => x.ProductionAccommodationBuildingId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}