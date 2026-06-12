namespace Alakai.FestivalManager.Admin.Services;

public class FestivalApiClient
{
    private readonly HttpClient _httpClient;

    public FestivalApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<FestivalDto>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<FestivalDto>>>(
            "https://localhost:7157/api/festivals");

        return response?.Data ?? [];
    }
}
