using Alakai.FestivalManager.Application.Features.EmailLayouts.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.EmailLayouts.Contracts.Requests;
using Alakai.FestivalManager.Application.Features.EmailLayouts.Contracts.Responses;
using System.Text.Json;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class EmailLayoutApiClient
{
    private readonly HttpClient _httpClient;

    public EmailLayoutApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<EmailLayoutDto> GetAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetEmailLayoutResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEmailLayoutResponse>>("api/email-layout", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load email layout.", response?.Errors);
        }

        return response.Data?.EmailLayout ?? new EmailLayoutDto();
    }

    public async Task<EmailLayoutDto> UpdateAsync(UpdateEmailLayoutRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync("api/email-layout", request, cancellationToken);
        ApiResponse<UpdateEmailLayoutResponse>? response = await ReadResponseAsync<UpdateEmailLayoutResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);

        return response!.Data!.EmailLayout;
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