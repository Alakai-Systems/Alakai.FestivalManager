namespace Alakai.FestivalManager.Admin.Services.Api;

public class PaymentApiClient
{
    private readonly HttpClient _httpClient;

    public PaymentApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RedsysPaymentFormDto> CreateSessionAsync(CreatePaymentSessionRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/payments/session", request, cancellationToken);
        ApiResponse<RedsysPaymentFormDto>? response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<RedsysPaymentFormDto>>(cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            throw new ApiClientException(response?.Errors is { Count: > 0 } ? response.Errors[0] : "Could not start the payment.", response?.Errors);
        }

        return response.Data;
    }

    public async Task<bool> ProcessReturnAsync(string merchantParameters, CancellationToken cancellationToken = default)
    {
        ProcessRedsysReturnRequest request = new() { MerchantParameters = merchantParameters };
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/payments/redsys/process-return", request, cancellationToken);
        ApiResponse<bool>? result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken);
        return result?.Data is true;
    }

    public async Task ConfirmReturnAsync(ConfirmRedsysReturnRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/payments/redsys/confirm-return", request, cancellationToken);
        ApiResponse<bool>? response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Errors is { Count: > 0 } ? response.Errors[0] : "The payment confirmation could not be validated.", response?.Errors);
        }
    }
}
