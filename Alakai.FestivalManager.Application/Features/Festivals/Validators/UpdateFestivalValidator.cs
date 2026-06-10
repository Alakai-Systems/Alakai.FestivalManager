namespace Alakai.FestivalManager.Application.Features.Festivals.Validators;

public class UpdateFestivalValidator : AbstractValidator<UpdateFestivalCommand>
{
    public UpdateFestivalValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Festival name is required.")
            .MaximumLength(150);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Festival slug is required.")
            .MaximumLength(150)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Festival slug must be URL-friendly.");

        RuleFor(x => x.Website)
            .MaximumLength(300);

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500);
    }
}