namespace Alakai.FestivalManager.Admin.Contracts.Auth.Requests;

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}