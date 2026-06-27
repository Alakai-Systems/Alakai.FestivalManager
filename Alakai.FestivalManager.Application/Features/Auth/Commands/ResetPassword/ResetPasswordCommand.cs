namespace Alakai.FestivalManager.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommand
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
