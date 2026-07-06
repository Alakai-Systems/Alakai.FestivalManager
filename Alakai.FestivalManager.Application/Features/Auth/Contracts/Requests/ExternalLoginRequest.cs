namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Requests;

public class ExternalLoginRequest
{
    public string Provider { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
