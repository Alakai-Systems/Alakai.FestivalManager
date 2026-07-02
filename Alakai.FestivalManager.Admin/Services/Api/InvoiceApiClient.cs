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
}