using Alakai.FestivalManager.Admin.Services.Auth;
using System.Net.Http.Headers;
namespace Alakai.FestivalManager.Admin.Services.Api;

public class EmailNotificationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAdminTokenProvider _adminTokenProvider;

    public EmailNotificationApiClient(HttpClient httpClient, IAdminTokenProvider adminTokenProvider)
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

    public async Task SendRegistrationEmailAsync(Guid registrationId, EmailTemplateKey templateKey, CancellationToken cancellationToken = default)
    {
        await AttachAuthHeaderAsync();

        HttpResponseMessage httpResponse = await _httpClient.PostAsync($"api/emails/registrations/{registrationId}/{templateKey}/send", null, cancellationToken);

        ApiResponse<EmailLogDto>? response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<EmailLogDto>>(cancellationToken);

        if (httpResponse.IsSuccessStatusCode && response?.Success is true)
        {
            return;
        }

        string message = response?.Errors?.FirstOrDefault() ?? response?.Message ?? "Email could not be sent.";

        throw new Exception(message);
    }

    public Task SendRegistrationConfirmationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return SendRegistrationEmailAsync(registrationId, EmailTemplateKey.RegistrationCreated, cancellationToken);
    }

    public Task SendPaymentConfirmationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return SendRegistrationEmailAsync(registrationId, EmailTemplateKey.PaymentConfirmed, cancellationToken);
    }

    public Task SendPaymentReminderAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return SendRegistrationEmailAsync(registrationId, EmailTemplateKey.PaymentFailed, cancellationToken);
    }

    public Task SendMissingPartnerReminderAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return SendRegistrationEmailAsync(registrationId, EmailTemplateKey.WaitingPartner, cancellationToken);
    }

    public Task SendCancellationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return SendRegistrationEmailAsync(registrationId, EmailTemplateKey.RegistrationCancelled, cancellationToken);
    }
}