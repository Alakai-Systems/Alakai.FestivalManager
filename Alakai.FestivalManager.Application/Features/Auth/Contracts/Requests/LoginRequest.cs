namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Requests;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
