namespace Alakai.FestivalManager.Application.Features.Auth.Validators;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(r => r.Token).NotEmpty();
        RuleFor(r => r.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(100);
    }
}
