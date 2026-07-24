using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;
namespace Alakai.FestivalManager.Admin.Services.Api;

public class LevelApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public LevelApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
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

    public async Task<IReadOnlyList<LevelDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetLevelsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetLevelsResponse>>("api/levels", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load levels.", response?.Errors);
        }

        return response.Data?.Levels ?? [];
    }

    public async Task<IReadOnlyList<LevelDto>> GetByPassTypeIdAsync(Guid passTypeId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetLevelsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetLevelsResponse>>($"api/levels/by-pass-type/{passTypeId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load levels for pass type.", response?.Errors);
        }

        return response.Data?.Levels ?? [];
    }

    public async Task<LevelDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        ApiResponse<GetLevelByIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetLevelByIdResponse>>($"api/levels/{id}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load level.", response?.Errors);
        }

        return response.Data!.Level;
    }

    public async Task CreateAsync(CreateLevelRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/levels", request, cancellationToken);
        ApiResponse<CreateLevelResponse>? response = await ReadResponseAsync<CreateLevelResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateLevelRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/levels/{id}", request, cancellationToken);
        ApiResponse<UpdateLevelResponse>? response = await ReadResponseAsync<UpdateLevelResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/levels/{id}", cancellationToken);
        ApiResponse<DeleteLevelResponse>? response = await ReadResponseAsync<DeleteLevelResponse>(httpResponse, cancellationToken);

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