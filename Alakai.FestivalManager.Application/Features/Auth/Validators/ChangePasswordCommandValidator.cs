namespace Alakai.FestivalManager.Application.Features.Auth.Validators;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.CurrentPassword).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(c => c.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(100);
    }
}
