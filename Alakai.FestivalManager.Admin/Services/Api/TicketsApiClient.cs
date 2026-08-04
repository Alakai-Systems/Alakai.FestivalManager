using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class TicketsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public TicketsApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
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

    public async Task<ApiResponse<TicketCheckInResultDto>> CheckInAsync(string token, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/tickets/checkin", new CheckInTicketRequest { Token = token }, cancellationToken);

        ApiResponse<TicketCheckInResultDto>? response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<TicketCheckInResultDto>>(cancellationToken: cancellationToken);

        return response ?? new ApiResponse<TicketCheckInResultDto>
        {
            Success = false,
            Message = "Unexpected error contacting the server.",
            Data = null,
            Errors = ["No response received."]
        };
    }
}