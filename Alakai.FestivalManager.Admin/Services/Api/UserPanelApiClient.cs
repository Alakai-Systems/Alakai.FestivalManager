namespace Alakai.FestivalManager.Admin.Services.Api;

public class UserPanelApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStorageService _tokenStorageService;

    public UserPanelApiClient(HttpClient httpClient, ITokenStorageService tokenStorageService)
    {
        _httpClient = httpClient;
        _tokenStorageService = tokenStorageService;
    }

    public async Task<UserPanelDashboardDto?> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        ApiResponse<GetUserPanelDashboardResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>("api/user-panel/dashboard", cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Dashboard;
    }

    public async Task<UserPanelDashboardDto?> UpdateProfileAsync(UpdateUserPanelProfileRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync("api/user-panel/profile", request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true)
        {
            return null;
        }

        return response.Data?.Dashboard;
    }
}
