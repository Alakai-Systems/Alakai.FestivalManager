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

    public async Task ForgotPasswordAsync(string email)
    {
        await _httpClient.PostAsJsonAsync("api/auth/forgot-password",
            new
            {
                Email = email
            });
    }

    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

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

    public async Task ResetPasswordAsync(string token, string password)
    {
        await _httpClient.PostAsJsonAsync("api/auth/reset-password",
            new
            {
                Token = token,
                Password = password
            });
    }
}
