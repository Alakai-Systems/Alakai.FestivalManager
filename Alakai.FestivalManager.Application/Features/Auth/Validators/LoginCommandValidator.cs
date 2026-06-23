namespace Alakai.FestivalManager.Application.Features.Auth.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(l => l.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(l => l.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
    }
}
