using System.Text.Json;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class EmailLayoutApiClient
{
    private readonly HttpClient _httpClient;

    public EmailLayoutApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<EmailLayoutDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetEmailLayoutsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEmailLayoutsResponse>>("api/email-layout", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load email layouts.", response?.Errors);
        }

        return response.Data?.EmailLayouts ?? [];
    }

    public async Task CreateAsync(CreateEmailLayoutRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/email-layout", request, cancellationToken);
        ApiResponse<CreateEmailLayoutResponse>? response = await ReadResponseAsync<CreateEmailLayoutResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateEmailLayoutRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/email-layout/{id}", request, cancellationToken);
        ApiResponse<UpdateEmailLayoutResponse>? response = await ReadResponseAsync<UpdateEmailLayoutResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/email-layout/{id}", cancellationToken);
        ApiResponse<DeleteEmailLayoutResponse>? response = await ReadResponseAsync<DeleteEmailLayoutResponse>(httpResponse, cancellationToken);

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