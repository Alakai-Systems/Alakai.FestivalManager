namespace Alakai.FestivalManager.Application.Features.Auth.Validators;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(r => r.AccessToken).NotEmpty();
        RuleFor(r => r.RefreshToken).NotEmpty();
    }
}
