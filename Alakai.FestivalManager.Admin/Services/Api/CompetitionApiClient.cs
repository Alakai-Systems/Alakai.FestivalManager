using System.Text.Json;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class CompetitionApiClient
{
    private readonly HttpClient _httpClient;

    public CompetitionApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<CompetitionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetCompetitionsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetCompetitionsResponse>>("api/competitions", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load competitions.", response?.Errors);
        }

        return response.Data?.Competitions ?? [];
    }

    public async Task<IReadOnlyList<CompetitionDto>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetCompetitionsByEditionIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetCompetitionsByEditionIdResponse>>($"api/competitions/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load competitions.", response?.Errors);
        }

        return response.Data?.Competitions ?? [];
    }

    public async Task CreateAsync(CreateCompetitionRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/competitions", request, cancellationToken);
        ApiResponse<CreateCompetitionResponse>? response = await ReadResponseAsync<CreateCompetitionResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateCompetitionRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/competitions/{id}", request, cancellationToken);
        ApiResponse<UpdateCompetitionResponse>? response = await ReadResponseAsync<UpdateCompetitionResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/competitions/{id}", cancellationToken);
        ApiResponse<DeleteCompetitionResponse>? response = await ReadResponseAsync<DeleteCompetitionResponse>(httpResponse, cancellationToken);

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
