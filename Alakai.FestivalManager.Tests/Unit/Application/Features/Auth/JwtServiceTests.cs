namespace Alakai.FestivalManager.Tests.Unit.Application.Features.Auth;

public class JwtServiceTests
{
    private readonly JwtService _sut;
    private readonly JwtSettings _settings = new()
    {
        SecretKey = "super-secret-key-min-32-chars-long!!",
        Issuer = "alakai-test",
        Audience = "alakai-test-audience",
        ExpirationMinutes = 60,
        RefreshTokenExpirationDays = 7
    };

    public JwtServiceTests()
    {
        _sut = new JwtService(Options.Create(_settings));
    }

    private static User BuildUser() => new()
    {
        FirstName = "Mago",
        LastName = "Alakai",
        Email = "mago@alakai.com",
        Role = UserRole.Admin,
        IsActive = true
    };

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyString()
    {
        string token = _sut.GenerateAccessToken(BuildUser());
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateAccessToken_ContainsCorrectEmailClaim()
    {
        User user = BuildUser();
        string token = _sut.GenerateAccessToken(user);

        JwtSecurityToken parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        parsed.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
    }

    [Fact]
    public void GenerateAccessToken_ContainsCorrectRoleClaim()
    {
        User user = BuildUser();
        string token = _sut.GenerateAccessToken(user);

        JwtSecurityToken parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        parsed.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == user.Role.ToString());
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectIssuerAndAudience()
    {
        string token = _sut.GenerateAccessToken(BuildUser());

        JwtSecurityToken parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        parsed.Issuer.Should().Be(_settings.Issuer);
        parsed.Audiences.Should().Contain(_settings.Audience);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsTokenWithCorrectUserId()
    {
        User user = BuildUser();
        RefreshToken token = _sut.GenerateRefreshToken(user);

        token.UserId.Should().Be(user.Id);
        token.Token.Should().NotBeNullOrWhiteSpace();
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateRefreshToken_TwoCallsProduceDifferentTokens()
    {
        User user = BuildUser();
        RefreshToken t1 = _sut.GenerateRefreshToken(user);
        RefreshToken t2 = _sut.GenerateRefreshToken(user);

        t1.Token.Should().NotBe(t2.Token);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WhenValidToken_ReturnsPrincipal()
    {
        string token = _sut.GenerateAccessToken(BuildUser());
        ClaimsPrincipal? principal = _sut.GetPrincipalFromExpiredToken(token);

        principal.Should().NotBeNull();
        principal!.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WhenInvalidToken_ReturnsNull()
    {
        ClaimsPrincipal? principal = _sut.GetPrincipalFromExpiredToken("not.a.jwt");
        principal.Should().BeNull();
    }

    [Fact]
    public void GetAccessTokenExpiration_IsInFuture()
    {
        DateTime expiry = _sut.GetAccessTokenExpiration();
        expiry.Should().BeAfter(DateTime.UtcNow);
    }
}