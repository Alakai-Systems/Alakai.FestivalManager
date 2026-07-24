using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;
namespace Alakai.FestivalManager.Admin.Services.Api;

public class UploadImageResult
{
    public string Url { get; set; } = string.Empty;
}

public class UploadImageDetailResult
{
    public string Url { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}

public class GalleryImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UploadsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public UploadsApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
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

    public async Task<UploadImageDetailResult> UploadImageWithDetailsAsync(Stream content, string fileName, string contentType, Guid? festivalId, int? width = null, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        using MultipartFormDataContent form = new();
        using StreamContent streamContent = new(content);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        form.Add(streamContent, "file", fileName);

        if (width.HasValue)
        {
            form.Add(new StringContent(width.Value.ToString()), "width");
        }

        if (festivalId.HasValue)
        {
            form.Add(new StringContent(festivalId.Value.ToString()), "festivalId");
        }

        HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/uploads/images", form, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            string errorBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiClientException($"Image upload failed: {errorBody}", null);
        }

        UploadImageDetailResult? result = await httpResponse.Content.ReadFromJsonAsync<UploadImageDetailResult>(cancellationToken: cancellationToken);

        if (result is null || string.IsNullOrWhiteSpace(result.Url))
        {
            throw new ApiClientException("Image upload returned an empty URL.", null);
        }

        return result;
    }

    public async Task<List<GalleryImageDto>> GetGalleryAsync(Guid festivalId, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        List<GalleryImageDto>? result = await _httpClient.GetFromJsonAsync<List<GalleryImageDto>>($"api/uploads/gallery?festivalId={festivalId}", cancellationToken);
        return result ?? [];
    }

    public async Task DeleteGalleryImageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        await _httpClient.DeleteAsync($"api/uploads/gallery/{id}", cancellationToken);
    }

    public async Task<string> UploadImageAsync(Stream content, string fileName, string contentType, int? width = null, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        using MultipartFormDataContent form = new();
        using StreamContent streamContent = new(content);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        form.Add(streamContent, "file", fileName);

        if (width.HasValue)
        {
            form.Add(new StringContent(width.Value.ToString()), "width");
        }

        HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/uploads/images", form, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            string errorBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiClientException($"Image upload failed: {errorBody}", null);
        }

        UploadImageResult? result = await httpResponse.Content.ReadFromJsonAsync<UploadImageResult>(cancellationToken: cancellationToken);

        if (result is null || string.IsNullOrWhiteSpace(result.Url))
        {
            throw new ApiClientException("Image upload returned an empty URL.", null);
        }

        return result.Url;
    }
}