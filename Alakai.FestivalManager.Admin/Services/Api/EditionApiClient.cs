using System.Text.Json;
using Alakai.FestivalManager.Admin.Contracts.Editions.Requests;
using Alakai.FestivalManager.Admin.Contracts.Editions.Responses;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class EditionApiClient
{
    private readonly HttpClient _httpClient;

    public EditionApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<EditionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetEditionsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEditionsResponse>>("api/editions", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load editions.", response?.Errors);
        }

        return response.Data?.Editions ?? [];
    }

    public async Task<IReadOnlyList<EditionDto>> GetByFestivalIdAsync(Guid festivalId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetEditionsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEditionsResponse>>($"api/editions/by-festival/{festivalId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load editions for festival.", response?.Errors);
        }

        return response.Data?.Editions ?? [];
    }

    public async Task<EditionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetEditionByIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEditionByIdResponse>>($"api/editions/{id}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load edition.", response?.Errors);
        }

        return response.Data!.Edition;
    }

    public async Task CreateAsync(CreateEditionRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/editions", request, cancellationToken);
        ApiResponse<CreateEditionResponse>? response = await ReadResponseAsync<CreateEditionResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateEditionRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/editions/{id}", request, cancellationToken);
        ApiResponse<UpdateEditionResponse>? response = await ReadResponseAsync<UpdateEditionResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/editions/{id}", cancellationToken);
        ApiResponse<DeleteEditionResponse>? response = await ReadResponseAsync<DeleteEditionResponse>(httpResponse, cancellationToken);

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
