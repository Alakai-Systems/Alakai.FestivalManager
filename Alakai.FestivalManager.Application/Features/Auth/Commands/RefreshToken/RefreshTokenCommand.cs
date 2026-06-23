namespace Alakai.FestivalManager.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
