using Microsoft.AspNetCore.Components.Authorization;

namespace Alakai.FestivalManager.Admin.Services.Auth;

public class AdminTokenProvider : IAdminTokenProvider
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IAuthApiClient _authApiClient;

    private string? _accessToken;
    private string? _refreshToken;
    private DateTime? _accessTokenExpiresAtUtc;
    private bool _initialized;

    public AdminTokenProvider(AuthenticationStateProvider authenticationStateProvider, IAuthApiClient authApiClient)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _authApiClient = authApiClient;
    }

    public async Task<string?> GetValidAccessTokenAsync()
    {
        if (!_initialized)
        {
            await InitializeFromCookieAsync();
        }

        if (string.IsNullOrWhiteSpace(_accessToken))
        {
            return null;
        }

        bool isExpiredOrNearExpiry = !_accessTokenExpiresAtUtc.HasValue
            || _accessTokenExpiresAtUtc.Value <= DateTime.UtcNow.AddMinutes(1);

        if (!isExpiredOrNearExpiry)
        {
            return _accessToken;
        }

        if (string.IsNullOrWhiteSpace(_refreshToken))
        {
            return _accessToken;
        }

        try
        {
            AuthResultDto? refreshed = await _authApiClient.RefreshTokenAsync(_accessToken, _refreshToken);

            if (refreshed is not null)
            {
                _accessToken = refreshed.AccessToken;
                _refreshToken = refreshed.RefreshToken;
                _accessTokenExpiresAtUtc = refreshed.ExpiresAt;
            }
        }
        catch
        {
            // Refresh failed (e.g. refresh token also expired). Fall back to the token we have;
            // the API call will fail and the user will be prompted to log in again.
        }

        return _accessToken;
    }

    private async Task InitializeFromCookieAsync()
    {
        AuthenticationState authState = await _authenticationStateProvider.GetAuthenticationStateAsync();

        _accessToken = authState.User.FindFirst("access_token")?.Value;
        _refreshToken = authState.User.FindFirst("refresh_token")?.Value;

        string? expiresClaim = authState.User.FindFirst("access_token_expires")?.Value;

        if (DateTime.TryParse(expiresClaim, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime parsedExpiry))
        {
            _accessTokenExpiresAtUtc = parsedExpiry.ToUniversalTime();
        }

        _initialized = true;
    }
}