
namespace Alakai.FestivalManager.Admin.Services.Api;

public class FestivalModuleApiClient
{
    private readonly HttpClient _httpClient;

    public FestivalModuleApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int> GetEnabledModulesForRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        ApiResponse<RegistrationFestivalInfoDto>? response = await _httpClient.GetFromJsonAsync<ApiResponse<RegistrationFestivalInfoDto>>($"api/registrations/{registrationId}/festival-info", cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            return 0;
        }

        return response.Data.EnabledModules;
    }
}