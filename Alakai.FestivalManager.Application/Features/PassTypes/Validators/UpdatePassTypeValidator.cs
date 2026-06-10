namespace Alakai.FestivalManager.Application.Features.PassTypes.Validators;
public class UpdatePassTypeValidator : AbstractValidator<UpdatePassTypeCommand>
{
    public UpdatePassTypeValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.EditionId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
