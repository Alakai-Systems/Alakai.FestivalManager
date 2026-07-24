namespace Alakai.FestivalManager.Application.Features.ProductionBuildings.Validators;

public class CreateBuildingValidator : AbstractValidator<CreateBuildingCommand>
{
    public CreateBuildingValidator()
    {
        RuleFor(x => x.EditionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}