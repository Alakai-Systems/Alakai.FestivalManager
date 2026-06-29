namespace Alakai.FestivalManager.Admin.Services.Api;

public class UploadImageResult
{
    public string Url { get; set; } = string.Empty;
}

public class UploadsApiClient
{
    private readonly HttpClient _httpClient;

    public UploadsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> UploadImageAsync(Stream content, string fileName, string contentType, int? width = null, CancellationToken cancellationToken = default)
    {
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