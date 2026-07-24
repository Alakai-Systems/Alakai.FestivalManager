namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Validators;

public class UpdateProductionAccommodationZoneValidator : AbstractValidator<UpdateProductionAccommodationZoneCommand>
{
    public UpdateProductionAccommodationZoneValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ProductionAccommodationBuildingId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}