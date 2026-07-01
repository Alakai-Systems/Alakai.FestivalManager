namespace Alakai.FestivalManager.Admin.Contracts.Auth.Responses;

public class RefreshTokenResponse
{
    public AuthResultDto Auth { get; set; } = new();
}