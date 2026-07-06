using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Alakai.FestivalManager.Infrastructure.Auth;

public class ExternalAuthService : IExternalAuthService
{
    private const string GoogleTokenInfoEndpoint = "https://oauth2.googleapis.com/tokeninfo";
    private const string AppleIssuer = "https://appleid.apple.com";
    private const string AppleKeysEndpoint = "https://appleid.apple.com/auth/keys";

    private static readonly HttpClient HttpClient = new();
    private static readonly SemaphoreSlim AppleKeysLock = new(1, 1);
    private static JsonWebKeySet? _appleKeys;
    private static DateTime _appleKeysFetchedAt = DateTime.MinValue;

    private readonly ExternalAuthOptions _options;
    private readonly ILogger<ExternalAuthService> _logger;

    public ExternalAuthService(IOptions<ExternalAuthOptions> options, ILogger<ExternalAuthService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ExternalUserInfo?> ValidateTokenAsync(string provider, string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            return provider.Trim().ToLowerInvariant() switch
            {
                "google" => await ValidateGoogleTokenAsync(token, cancellationToken),
                "apple" => await ValidateAppleTokenAsync(token, cancellationToken),
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "External token validation failed for provider {Provider}.", provider);

            return null;
        }
    }

    private async Task<ExternalUserInfo?> ValidateGoogleTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Google.ClientId))
        {
            _logger.LogWarning("Google external login attempted but ExternalAuth:Google:ClientId is not configured.");

            return null;
        }

        using HttpResponseMessage response = await HttpClient.GetAsync($"{GoogleTokenInfoEndpoint}?access_token={Uri.EscapeDataString(accessToken)}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        JsonElement root = document.RootElement;

        string? audience = root.TryGetProperty("aud", out JsonElement audElement) ? audElement.GetString() : null;

        if (!string.Equals(audience, _options.Google.ClientId, StringComparison.Ordinal))
        {
            _logger.LogWarning("Google token audience mismatch.");

            return null;
        }

        string? email = root.TryGetProperty("email", out JsonElement emailElement) ? emailElement.GetString() : null;

        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        bool emailVerified = root.TryGetProperty("email_verified", out JsonElement verifiedElement)
            && string.Equals(verifiedElement.GetString(), "true", StringComparison.OrdinalIgnoreCase);

        return new ExternalUserInfo { Provider = "google", Email = email, EmailVerified = emailVerified };
    }

    private async Task<ExternalUserInfo?> ValidateAppleTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Apple.ClientId))
        {
            _logger.LogWarning("Apple external login attempted but ExternalAuth:Apple:ClientId is not configured.");

            return null;
        }

        JsonWebKeySet keys = await GetAppleKeysAsync(cancellationToken);

        TokenValidationParameters parameters = new()
        {
            ValidIssuer = AppleIssuer,
            ValidAudience = _options.Apple.ClientId,
            IssuerSigningKeys = keys.GetSigningKeys(),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        JwtSecurityTokenHandler handler = new();
        ClaimsPrincipal principal = handler.ValidateToken(idToken, parameters, out _);

        string? email = principal.FindFirst("email")?.Value;

        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        string? verifiedClaim = principal.FindFirst("email_verified")?.Value;
        bool emailVerified = verifiedClaim is null || string.Equals(verifiedClaim, "true", StringComparison.OrdinalIgnoreCase);

        return new ExternalUserInfo { Provider = "apple", Email = email, EmailVerified = emailVerified };
    }

    private async Task<JsonWebKeySet> GetAppleKeysAsync(CancellationToken cancellationToken)
    {
        if (_appleKeys is not null && DateTime.UtcNow - _appleKeysFetchedAt < TimeSpan.FromHours(24))
        {
            return _appleKeys;
        }

        await AppleKeysLock.WaitAsync(cancellationToken);

        try
        {
            if (_appleKeys is null || DateTime.UtcNow - _appleKeysFetchedAt >= TimeSpan.FromHours(24))
            {
                string json = await HttpClient.GetStringAsync(AppleKeysEndpoint, cancellationToken);
                _appleKeys = new JsonWebKeySet(json);
                _appleKeysFetchedAt = DateTime.UtcNow;
            }

            return _appleKeys;
        }
        finally
        {
            AppleKeysLock.Release();
        }
    }
}
