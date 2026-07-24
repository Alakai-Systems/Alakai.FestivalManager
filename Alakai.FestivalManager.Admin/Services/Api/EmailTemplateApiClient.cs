using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class EmailTemplatePreviewResult
{
    public string Subject { get; set; } = string.Empty;
    public string Html { get; set; } = string.Empty;
}

public class EmailTemplateApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public EmailTemplateApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
    {
        _httpClient = httpClient;
        _adminTokenProvider = adminTokenProvider;
    }

    private async Task AttachAuthHeaderAsync()
    {
        string? adminToken = await _adminTokenProvider.GetValidAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(adminToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        }
    }

    public async Task<IReadOnlyList<EmailTemplateDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetEmailTemplatesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEmailTemplatesResponse>>("api/email-templates", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load email templates.", response?.Errors);
        }

        return response.Data?.EmailTemplates ?? [];
    }

    public async Task<IReadOnlyList<EmailTemplateDto>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetEmailTemplatesByEditionIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEmailTemplatesByEditionIdResponse>>($"api/email-templates/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load email templates.", response?.Errors);
        }

        return response.Data?.EmailTemplates ?? [];
    }

    public async Task<EmailTemplatePreviewResult?> PreviewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        return await _httpClient.GetFromJsonAsync<EmailTemplatePreviewResult>($"api/email-templates/{id}/preview", cancellationToken);
    }

    public async Task CreateAsync(CreateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/email-templates", request, cancellationToken);
        ApiResponse<CreateEmailTemplateResponse>? response = await ReadResponseAsync<CreateEmailTemplateResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/email-templates/{id}", request, cancellationToken);
        ApiResponse<UpdateEmailTemplateResponse>? response = await ReadResponseAsync<UpdateEmailTemplateResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

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
