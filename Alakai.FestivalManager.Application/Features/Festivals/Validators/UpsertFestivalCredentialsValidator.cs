namespace Alakai.FestivalManager.Application.Features.Festivals.Validators;

public class UpsertFestivalCredentialsValidator : AbstractValidator<UpsertFestivalCredentialsCommand>
{
    public UpsertFestivalCredentialsValidator()
    {
        RuleFor(x => x.RedsysMerchantCode)
            .NotEmpty()
            .WithMessage("Redsys merchant code is required.")
            .MaximumLength(50);

        RuleFor(x => x.RedsysTerminal)
            .NotEmpty()
            .WithMessage("Redsys terminal is required.")
            .MaximumLength(10);

        RuleFor(x => x.RedsysMerchantName)
            .NotEmpty()
            .WithMessage("Redsys merchant name is required.")
            .MaximumLength(150);

        RuleFor(x => x.EmailHost)
            .NotEmpty()
            .WithMessage("Email host is required.")
            .MaximumLength(200);

        RuleFor(x => x.EmailPort)
            .GreaterThan(0)
            .WithMessage("Email port must be a positive number.");

        RuleFor(x => x.EmailUsername)
            .NotEmpty()
            .WithMessage("Email username is required.")
            .MaximumLength(200);

        RuleFor(x => x.EmailFromEmail)
            .NotEmpty()
            .WithMessage("Email from-address is required.")
            .MaximumLength(200);

        RuleFor(x => x.EmailFromName)
            .NotEmpty()
            .WithMessage("Email from-name is required.")
            .MaximumLength(150);
    }
}