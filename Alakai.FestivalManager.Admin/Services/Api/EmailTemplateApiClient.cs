using System.Text.Json;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class EmailTemplateApiClient
{
    private readonly HttpClient _httpClient;

    public EmailTemplateApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<EmailTemplateDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetEmailTemplatesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEmailTemplatesResponse>>("api/email-templates", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load email templates.", response?.Errors);
        }

        return response.Data?.EmailTemplates ?? [];
    }

    public async Task<IReadOnlyList<EmailTemplateDto>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetEmailTemplatesByEditionIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEmailTemplatesByEditionIdResponse>>($"api/email-templates/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load email templates.", response?.Errors);
        }

        return response.Data?.EmailTemplates ?? [];
    }

    public async Task CreateAsync(CreateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/email-templates", request, cancellationToken);
        ApiResponse<CreateEmailTemplateResponse>? response = await ReadResponseAsync<CreateEmailTemplateResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/email-templates/{id}", request, cancellationToken);
        ApiResponse<UpdateEmailTemplateResponse>? response = await ReadResponseAsync<UpdateEmailTemplateResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/email-templates/{id}", cancellationToken);
        ApiResponse<DeleteEmailTemplateResponse>? response = await ReadResponseAsync<DeleteEmailTemplateResponse>(httpResponse, cancellationToken);

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
