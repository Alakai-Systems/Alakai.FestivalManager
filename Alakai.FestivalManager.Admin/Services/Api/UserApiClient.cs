using System.Text.Json;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class UserApiClient
{
    private readonly HttpClient _httpClient;

    public UserApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetUsersResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetUsersResponse>>("api/users", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load users.", response?.Errors);
        }

        return response.Data?.Users ?? [];
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetUserByIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetUserByIdResponse>>($"api/users/{id}", cancellationToken);

        if (response?.Success is not true || response.Data?.User is null)
        {
            throw new ApiClientException(response?.Message ?? "Could not load user.", response?.Errors);
        }

        return response.Data.User;
    }

    public async Task CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/users", request, cancellationToken);
        ApiResponse<CreateUserResponse>? response = await ReadResponseAsync<CreateUserResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/users/{id}", request, cancellationToken);
        ApiResponse<UpdateUserResponse>? response = await ReadResponseAsync<UpdateUserResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/users/{id}", cancellationToken);
        ApiResponse<DeleteUserResponse>? response = await ReadResponseAsync<DeleteUserResponse>(httpResponse, cancellationToken);

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
