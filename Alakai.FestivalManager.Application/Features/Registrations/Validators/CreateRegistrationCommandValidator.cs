using Alakai.FestivalManager.Application.Features.Registrations.Commands.CreateRegistration;
using FluentValidation;

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
        RuleFor(x => x.PartnerEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.PartnerEmail));
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FinalPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountCode).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.InternalNotes).MaximumLength(2000);
    }
}
