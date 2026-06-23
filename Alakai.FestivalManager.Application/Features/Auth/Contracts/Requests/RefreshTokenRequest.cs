namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Requests;

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
