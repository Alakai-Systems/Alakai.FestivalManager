using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;
using Alakai.FestivalManager.Admin.Contracts.Invoices.DTOs;
using Alakai.FestivalManager.Admin.Contracts.Invoices.Responses;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class InvoiceSettingsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public InvoiceSettingsApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
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

    public async Task<InvoiceSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetInvoiceSettingsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetInvoiceSettingsResponse>>("api/invoice-settings", cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            throw new ApiClientException(response?.Message ?? "Could not load invoice settings.", response?.Errors);
        }

        return response.Data.Settings;
    }

    public async Task UpdateAsync(InvoiceSettingsDto settings, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync("api/invoice-settings", settings, cancellationToken);

        ApiResponse<UpdateInvoiceSettingsResponse>? response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<UpdateInvoiceSettingsResponse>>(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode || response?.Success is not true)
        {
            string message = response?.Message ?? $"Request failed with status code {(int)httpResponse.StatusCode}.";
            throw new ApiClientException(message, response?.Errors);
        }
    }
}