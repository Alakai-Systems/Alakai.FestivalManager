namespace Alakai.FestivalManager.Admin.Contracts.Auth.Responses;

public class LoginResponse
{
    public AuthResultDto Auth { get; set; } = new();
    public string Language { get; set; } = "en";
}
