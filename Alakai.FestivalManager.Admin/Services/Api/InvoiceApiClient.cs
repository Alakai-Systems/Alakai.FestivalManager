using Alakai.FestivalManager.Admin.Contracts.Invoices.DTOs;
using Alakai.FestivalManager.Admin.Contracts.Invoices.Responses;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class InvoiceApiClient
{
    private readonly HttpClient _httpClient;

    public InvoiceApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetInvoicesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetInvoicesResponse>>("api/invoices", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load invoices.", response?.Errors);
        }

        return response.Data?.Invoices ?? [];
    }

    public async Task UpdateAsync(Guid id, UpdateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/invoices/{id}", request, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            string errorBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiClientException($"Could not update invoice: {errorBody}", null);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/invoices/{id}", cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            string errorBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiClientException($"Could not delete invoice: {errorBody}", null);
        }
    }
}

public class UpdateInvoiceRequest
{
    public string FiscalName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}