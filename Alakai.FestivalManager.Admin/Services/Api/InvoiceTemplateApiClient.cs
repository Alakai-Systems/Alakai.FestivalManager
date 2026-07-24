using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;
using Alakai.FestivalManager.Admin.Contracts.Invoices.DTOs;
using Alakai.FestivalManager.Admin.Contracts.Invoices.Requests;
using Alakai.FestivalManager.Admin.Contracts.Invoices.Responses;
using System.Text.Json;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class InvoiceTemplateApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public InvoiceTemplateApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
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

    public async Task<IReadOnlyList<InvoiceTemplateDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetInvoiceTemplatesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetInvoiceTemplatesResponse>>("api/invoice-templates", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load invoice templates.", response?.Errors);
        }

        return response.Data?.Templates ?? [];
    }

    public async Task CreateAsync(CreateInvoiceTemplateRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/invoice-templates", request, cancellationToken);
        ApiResponse<CreateInvoiceTemplateResponse>? response = await ReadResponseAsync<CreateInvoiceTemplateResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateInvoiceTemplateRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/invoice-templates/{id}", request, cancellationToken);
        ApiResponse<UpdateInvoiceTemplateResponse>? response = await ReadResponseAsync<UpdateInvoiceTemplateResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/invoice-templates/{id}", cancellationToken);
        ApiResponse<DeleteInvoiceTemplateResponse>? response = await ReadResponseAsync<DeleteInvoiceTemplateResponse>(httpResponse, cancellationToken);

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