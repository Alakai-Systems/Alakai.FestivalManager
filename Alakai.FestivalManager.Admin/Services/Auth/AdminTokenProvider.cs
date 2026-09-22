using Microsoft.AspNetCore.Components.Authorization;

namespace Alakai.FestivalManager.Admin.Services.Auth;

public class AdminTokenProvider : IAdminTokenProvider
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IAuthApiClient _authApiClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private string? _accessToken;
    private string? _refreshToken;
    private DateTime? _accessTokenExpiresAtUtc;
    private bool _initialized;

    public AdminTokenProvider(AuthenticationStateProvider authenticationStateProvider, IAuthApiClient authApiClient, IHttpContextAccessor httpContextAccessor)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _authApiClient = authApiClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public void ClearTokens()
    {
        _accessToken = null;
        _refreshToken = null;
        _accessTokenExpiresAtUtc = null;
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

                await TryPersistRefreshedTokensToCookieAsync(refreshed);
            }
        }
        catch
        {
            // Refresh failed (e.g. refresh token also expired). Fall back to the token we have;
            // the API call will fail and the user will be prompted to log in again.
        }

        return _accessToken;
    }

    // El refresco de arriba solo actualiza los campos en memoria de esta instancia
    // (una por circuito de Blazor). Si no se vuelve a guardar tambien en la cookie,
    // un circuito NUEVO (recarga de pagina, reconexion tras dormir el portatil, etc.)
    // seguira leyendo el refresh_token original del login, que el backend ya invalido
    // al usarse aqui (rotacion de refresh tokens) -> ese circuito nuevo no podra
    // refrescar nunca y todas sus llamadas a la API devolveran 401.
    //
    // Solo se puede reescribir la cookie cuando hay un HttpContext real disponible
    // (tipicamente al arrancar un circuito nuevo, durante el prerenderizado de una
    // peticion HTTP normal). Una vez el circuito interactivo ya esta conectado por
    // SignalR, HttpContext deja de estar disponible de forma fiable; en ese caso esto
    // no hace nada y el token refrescado sigue funcionando en memoria para el resto
    // de ese circuito.
    private async Task TryPersistRefreshedTokensToCookieAsync(AuthResultDto refreshed)
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null || httpContext.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        try
        {
            List<Claim> claims = httpContext.User.Claims
                .Where(claim => claim.Type is not ("access_token" or "refresh_token" or "access_token_expires"))
                .ToList();

            claims.Add(new Claim("access_token", refreshed.AccessToken));
            claims.Add(new Claim("refresh_token", refreshed.RefreshToken));
            claims.Add(new Claim("access_token_expires", refreshed.ExpiresAt.ToString("o")));

            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new(identity);

            AuthenticationProperties authProperties = new()
            {
                IsPersistent = true
            };

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        }
        catch
        {
            // La respuesta ya se habia empezado a enviar y no se puede reescribir la
            // cookie. El token refrescado sigue siendo valido en memoria para el resto
            // de este circuito.
        }
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