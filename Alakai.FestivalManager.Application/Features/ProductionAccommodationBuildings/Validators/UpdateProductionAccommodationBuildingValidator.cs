namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationBuildings.Validators;

public class UpdateProductionAccommodationBuildingValidator : AbstractValidator<UpdateProductionAccommodationBuildingCommand>
{
    public UpdateProductionAccommodationBuildingValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.EditionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}