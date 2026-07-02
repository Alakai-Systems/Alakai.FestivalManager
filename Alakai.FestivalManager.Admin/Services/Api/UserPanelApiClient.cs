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

    public async Task<UserPanelDashboardDto> CreateInvoiceAsync(CreateUserPanelInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("You are not logged in.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/user-panel/invoices", request, cancellationToken);

        ApiResponse<GetUserPanelDashboardResponse>? response =
            await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetUserPanelDashboardResponse>>(cancellationToken);

        if (response?.Success is not true || response.Data?.Dashboard is null)
        {
            string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Invoice could not be created.";
            throw new Exception(message);
        }

        return response.Data.Dashboard;
    }
}