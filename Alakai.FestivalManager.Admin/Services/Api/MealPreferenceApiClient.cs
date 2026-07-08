namespace Alakai.FestivalManager.Admin.Services.Api;

public class MealPreferenceApiClient
{
    private readonly HttpClient _httpClient;

    public MealPreferenceApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<MealPreferenceDto?> GetByRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetMealPreferenceResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetMealPreferenceResponse>>($"api/meal-preferences/by-registration/{registrationId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load the meal preference.", response?.Errors);
        }

        return response.Data?.Preference;
    }

    public async Task<IReadOnlyList<MealPreferenceDto>> GetByEditionAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetMealPreferencesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetMealPreferencesResponse>>($"api/meal-preferences/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load meal preferences.", response?.Errors);
        }

        return response.Data?.Preferences ?? [];
    }

    public async Task<MealPreferenceDto> SaveAsync(SaveMealPreferenceRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/meal-preferences", request, cancellationToken);
        ApiResponse<SaveMealPreferenceResponse>? response = await ReadResponseAsync<SaveMealPreferenceResponse>(httpResponse, cancellationToken);
        EnsureSuccess(httpResponse, response);
        return response!.Data!.Preference;
    }

    private static async Task<ApiResponse<T>?> ReadResponseAsync<T>(HttpResponseMessage httpResponse, CancellationToken cancellationToken)
    {
        try
        {
            return await httpResponse.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken);
        }
        catch (JsonException)
        {
            string content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            string message = string.IsNullOrWhiteSpace(content) ? $"Request failed with status code {(int)httpResponse.StatusCode}." : content;
            throw new ApiClientException(message);
        }
    }

    private static void EnsureSuccess<T>(HttpResponseMessage httpResponse, ApiResponse<T>? response)
    {
        if (httpResponse.IsSuccessStatusCode && response?.Success == true)
        {
            return;
        }

        string message = response?.Message ?? $"Request failed with status code {(int)httpResponse.StatusCode}.";
        IReadOnlyList<string> errors = response?.Errors ?? [];
        throw new ApiClientException(message, errors);
    }
}