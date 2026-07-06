namespace Alakai.FestivalManager.Application.Features.Auth.Commands.ExternalLogin;

public class ExternalLoginCommand
{
    public string Provider { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
