namespace Alakai.FestivalManager.Application.Features.Auth.Validators;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(f => f.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}
