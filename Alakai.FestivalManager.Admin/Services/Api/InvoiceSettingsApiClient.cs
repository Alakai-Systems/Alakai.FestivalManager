using Alakai.FestivalManager.Admin.Contracts.Invoices.DTOs;
using Alakai.FestivalManager.Admin.Contracts.Invoices.Responses;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class InvoiceSettingsApiClient
{
    private readonly HttpClient _httpClient;

    public InvoiceSettingsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<InvoiceSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetInvoiceSettingsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetInvoiceSettingsResponse>>("api/invoice-settings", cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            throw new ApiClientException(response?.Message ?? "Could not load invoice settings.", response?.Errors);
        }

        return response.Data.Settings;
    }

    public async Task UpdateAsync(InvoiceSettingsDto settings, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync("api/invoice-settings", settings, cancellationToken);

        ApiResponse<UpdateInvoiceSettingsResponse>? response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<UpdateInvoiceSettingsResponse>>(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode || response?.Success is not true)
        {
            string message = response?.Message ?? $"Request failed with status code {(int)httpResponse.StatusCode}.";
            throw new ApiClientException(message, response?.Errors);
        }
    }
}