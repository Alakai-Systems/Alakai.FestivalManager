namespace Alakai.FestivalManager.Application.Features.Competitions.Validators;

public class CreateCompetitionCommandValidator : AbstractValidator<CreateCompetitionCommand>
{
    public CreateCompetitionCommandValidator()
    {
        RuleFor(c => c.EditionId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(150);
        RuleFor(c => c.Description).MaximumLength(1000);
        RuleFor(c => c.Price).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Format).NotEmpty();
        RuleFor(c => c.MaxParticipants).GreaterThan(0).When(c => c.MaxParticipants.HasValue);
    }
}
