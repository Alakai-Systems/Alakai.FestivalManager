namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Requests;

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
