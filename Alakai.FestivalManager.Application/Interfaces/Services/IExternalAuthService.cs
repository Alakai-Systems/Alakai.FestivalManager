namespace Alakai.FestivalManager.Application.Interfaces.Services;

public interface IExternalAuthService
{
    Task<ExternalUserInfo?> ValidateTokenAsync(string provider, string token, CancellationToken cancellationToken = default);
}

public class ExternalUserInfo
{
    public string Provider { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
}
