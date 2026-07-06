namespace Alakai.FestivalManager.Admin.Services.Auth;

public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStorageService _tokenStorageService;

    public AuthApiClient(HttpClient httpClient, ITokenStorageService tokenStorageService)
    {
        _httpClient = httpClient;
        _tokenStorageService = tokenStorageService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/auth/login", request);

        string content = await httpResponse.Content.ReadAsStringAsync();

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception(content);
        }

        ApiResponse<LoginResponse>? response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

        if (response is null)
        {
            throw new Exception("Empty login response.");
        }

        if (!response.Success)
        {
            throw new Exception(response.Message);
        }

        if (response.Data is null)
        {
            throw new Exception("Login response data is null.");
        }

        return response.Data;
    }

    public async Task<LoginResponse?> ExternalLoginAsync(ExternalLoginRequest request)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/auth/external-login", request);

        ApiResponse<LoginResponse>? response = null;

        try
        {
            response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        }
        catch
        {
            // Non-JSON error payload; handled below.
        }

        if (response is null)
        {
            throw new Exception("External sign-in failed.");
        }

        if (!response.Success || response.Data is null)
        {
            string message = response.Errors is { Count: > 0 } ? response.Errors[0] : response.Message;

            throw new Exception(string.IsNullOrWhiteSpace(message) ? "External sign-in failed." : message);
        }

        return response.Data;
    }

    public async Task<string?> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        object request = new
        {
            Email = email
        };

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", request, cancellationToken);

        ApiResponse<object>? response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken);

        if (httpResponse.IsSuccessStatusCode && response?.Success is true)
        {
            return response.Message;
        }

        throw new Exception(response?.Errors?.FirstOrDefault()
            ?? response?.Message
            ?? "Password reset email could not be sent.");
    }

    public async Task<string?> ResetPasswordAsync(string token, string newPassword, string confirmPassword, CancellationToken cancellationToken = default)
    {
        object request = new
        {
            Token = token,
            NewPassword = newPassword,
            ConfirmPassword = confirmPassword
        };

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/auth/reset-password", request, cancellationToken);

        ApiResponse<object>? response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken);

        if (httpResponse.IsSuccessStatusCode && response?.Success is true)
        {
            return response.Message;
        }

        throw new Exception(response?.Errors?.FirstOrDefault()
            ?? response?.Message
            ?? "Password could not be reset.");
    }
    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword, string? accessToken = null)
    {
        string? token = accessToken ?? await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(
            "api/auth/change-password",
            new
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            });

        return httpResponse.IsSuccessStatusCode;
    }

    public async Task<AuthResultDto?> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default)
    {
        RefreshTokenRequest request = new()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/auth/refresh-token", request, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            return null;
        }

        ApiResponse<RefreshTokenResponse>? response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<RefreshTokenResponse>>(cancellationToken);

        if (response?.Success is not true || response.Data?.Auth is null)
        {
            return null;
        }

        return response.Data.Auth;
    }
}