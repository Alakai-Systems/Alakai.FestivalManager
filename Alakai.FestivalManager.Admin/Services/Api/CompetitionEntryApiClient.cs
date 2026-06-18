using System.Text.Json;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class CompetitionEntryApiClient
{
    private readonly HttpClient _httpClient;

    public CompetitionEntryApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<CompetitionEntryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetCompetitionEntriesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetCompetitionEntriesResponse>>("api/competition-entries", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load competition entries.", response?.Errors);
        }

        return response.Data?.CompetitionEntries ?? [];
    }

    public async Task<IReadOnlyList<CompetitionEntryDto>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetCompetitionEntriesByCompetitionIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetCompetitionEntriesByCompetitionIdResponse>>($"api/competition-entries/by-competition/{competitionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load competition entries.", response?.Errors);
        }

        return response.Data?.CompetitionEntries ?? [];
    }

    public async Task<IReadOnlyList<CompetitionEntryDto>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetCompetitionEntriesByRegistrationIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetCompetitionEntriesByRegistrationIdResponse>>($"api/competition-entries/by-registration/{registrationId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load competition entries.", response?.Errors);
        }

        return response.Data?.CompetitionEntries ?? [];
    }

    public async Task CreateAsync(CreateCompetitionEntryRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/competition-entries", request, cancellationToken);
        ApiResponse<CreateCompetitionEntryResponse>? response = await ReadResponseAsync<CreateCompetitionEntryResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateCompetitionEntryRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/competition-entries/{id}", request, cancellationToken);
        ApiResponse<UpdateCompetitionEntryResponse>? response = await ReadResponseAsync<UpdateCompetitionEntryResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/competition-entries/{id}", cancellationToken);
        ApiResponse<DeleteCompetitionEntryResponse>? response = await ReadResponseAsync<DeleteCompetitionEntryResponse>(httpResponse, cancellationToken);

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
