namespace Alakai.FestivalManager.Application.Features.Registrations.Validators;

public class CreateRegistrationCommandValidator : AbstractValidator<CreateRegistrationCommand>
{
    public CreateRegistrationCommandValidator()
    {
        RuleFor(x => x.EditionId).NotEmpty();
        RuleFor(x => x.PassTypeId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(r => r.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);
        RuleFor(r => r.DocumentNumber)
            .MaximumLength(100);
        RuleFor(r => r.DocumentCountry)
            .MaximumLength(100);
        RuleFor(x => x.PartnerEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.PartnerEmail));
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.InternalNotes).MaximumLength(2000);
    }
}
