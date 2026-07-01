namespace Alakai.FestivalManager.Admin.Endpoints;

public static class AdminAuthEndpoints
{
    public static void MapAdminAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/account/login", async (
            HttpContext httpContext,
            IAuthApiClient authApiClient,
            [FromForm] string email,
            [FromForm] string password,
            [FromForm] string? returnUrl) =>
        {
            try
            {
                LoginResponse? response = await authApiClient.LoginAsync(new LoginRequest { Email = email, Password = password });

                if (response?.Auth is null)
                {
                    return Results.Redirect("/login?error=invalid");
                }

                AuthUserDto user = response.Auth.User;
                AdminUserRole role = (AdminUserRole)user.Role;

                if (role != AdminUserRole.SuperAdmin && role != AdminUserRole.Admin)
                {
                    return Results.Redirect("/login?error=forbidden");
                }

                List<Claim> claims =
                [
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.Role, role.ToString()),
                    new("access_token", response.Auth.AccessToken),
                    new("refresh_token", response.Auth.RefreshToken),
                    new("access_token_expires", response.Auth.ExpiresAt.ToString("o"))
                ];

                ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal principal = new(identity);

                AuthenticationProperties authProperties = new()
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                string redirectTo = string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/')
                    ? "/dashboard"
                    : returnUrl;

                return Results.Redirect(redirectTo);
            }
            catch (Exception)
            {
                return Results.Redirect("/login?error=invalid");
            }
        }).AllowAnonymous();

        app.MapPost("/account/logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/login");
        }).AllowAnonymous();
    }
}