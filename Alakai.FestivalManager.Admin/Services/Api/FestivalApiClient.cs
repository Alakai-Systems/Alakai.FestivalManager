
using System.Text.Json;


namespace Alakai.FestivalManager.Admin.Services.Api;

public class FestivalApiClient
{
    private readonly HttpClient _httpClient;

    public FestivalApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<FestivalDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetFestivalsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetFestivalsResponse>>("api/festivals", cancellationToken);
        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load festivals.", response?.Errors);
        }

        return response.Data?.Festivals ?? [];
    }

    public async Task CreateAsync(CreateFestivalRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/festivals", request, cancellationToken);
        ApiResponse<CreateFestivalResponse>? response = await ReadResponseAsync<CreateFestivalResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateFestivalRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/festivals/{id}", request, cancellationToken);
        ApiResponse<UpdateFestivalResponse>? response = await ReadResponseAsync<UpdateFestivalResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/festivals/{id}", cancellationToken);
        ApiResponse<DeleteFestivalResponse>? response = await ReadResponseAsync<DeleteFestivalResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
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