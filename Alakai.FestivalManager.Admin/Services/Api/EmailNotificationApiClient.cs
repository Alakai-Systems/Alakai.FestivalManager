namespace Alakai.FestivalManager.Admin.Services.Api;

public class EmailNotificationApiClient
{
    private readonly HttpClient _httpClient;

    public EmailNotificationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendRegistrationEmailAsync(Guid registrationId, EmailTemplateKey templateKey, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsync($"api/emails/registrations/{registrationId}/{templateKey}/send", null, cancellationToken);

        ApiResponse<EmailLogDto>? response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<EmailLogDto>>(cancellationToken);

        if (httpResponse.IsSuccessStatusCode && response?.Success is true)
        {
            return;
        }

        string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Email could not be sent.";

        throw new Exception(message);
    }
}