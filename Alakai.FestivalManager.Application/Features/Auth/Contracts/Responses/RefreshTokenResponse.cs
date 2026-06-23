namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Responses;

public class RefreshTokenResponse
{
    public AuthResultDto Auth { get; set; } = default!;
}
