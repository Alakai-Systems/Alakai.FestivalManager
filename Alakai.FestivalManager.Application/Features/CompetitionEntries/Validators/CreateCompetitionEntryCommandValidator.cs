namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Validators;

public class CreateCompetitionEntryCommandValidator : AbstractValidator<CreateCompetitionEntryCommand>
{
    public CreateCompetitionEntryCommandValidator()
    {
        RuleFor(e => e.CompetitionId).NotEmpty();
        RuleFor(e => e.RegistrationId).NotEmpty();
        RuleFor(e => e.DanceRole).NotEmpty();
        RuleFor(e => e.CompetitionCapacityId).NotEmpty();
        RuleFor(e => e.Notes).MaximumLength(2000);
        RuleFor(e => e.InternalNotes).MaximumLength(2000);
    }
}
