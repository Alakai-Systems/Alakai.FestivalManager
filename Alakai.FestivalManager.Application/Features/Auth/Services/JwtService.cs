namespace Alakai.FestivalManager.Application.Features.Auth.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;

    public JwtService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateAccessToken(User user)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        ];

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);
        DateTime expiresAt = GetAccessTokenExpiration();

        JwtSecurityToken token = new(issuer: _jwtSettings.Issuer, audience: _jwtSettings.Audience, claims: claims, expires: expiresAt, signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(User user)
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
        string token = Convert.ToBase64String(randomBytes);

        return new RefreshToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        TokenValidationParameters parameters = new()
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey))
        };

        JwtSecurityTokenHandler handler = new();

        try
        {
            return handler.ValidateToken(token, parameters, out SecurityToken _);
        }
        catch
        {
            return null;
        }
    }

    public DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
    }
}
