using Alakai.FestivalManager.Application.Features.Registrations.Contracts.Responses;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class PublicRegistrationApiClient
{
    private readonly HttpClient _httpClient;

    public PublicRegistrationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PublicFestivalSlugDto?> GetFestivalBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<PublicFestivalSlugDto>($"api/public/festivals/by-slug/{slug}", cancellationToken);
    }

    public async Task<PublicEmailLayoutDto?> GetEmailLayoutAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PublicEmailLayoutDto>($"api/public/festivals/email-layout/{editionId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }




    public async Task<PublicFestivalBrandingDto?> GetFestivalByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PublicFestivalBrandingDto>($"api/public/festivals/by-domain/{domain}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<PublicRegistrationAvailabilityDto?> GetAvailabilityAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<PublicRegistrationAvailabilityDto>($"api/public/registrations/availability/{editionId}", cancellationToken);
    }

    public async Task<PublicPartnerLookupDto?> LookupPartnerAsync(Guid editionId, string email, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<PublicPartnerLookupDto>($"api/public/registrations/partner-lookup?editionId={editionId}&email={Uri.EscapeDataString(email)}", cancellationToken);
    }

    public async Task<PublicDiscountCheckDto?> CheckDiscountAsync(Guid editionId, string code, decimal basePrice, CancellationToken cancellationToken = default)
    {
        string amountStr = basePrice.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return await _httpClient.GetFromJsonAsync<PublicDiscountCheckDto>($"api/public/registrations/discount-check?editionId={editionId}&code={Uri.EscapeDataString(code)}&basePrice={amountStr}", cancellationToken);
    }

    public async Task<Guid?> CreateRegistrationAsync(CreateRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/registrations", request, cancellationToken);
        ApiResponse<CreateRegistrationResponse>? response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<CreateRegistrationResponse>>(cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            string message = response?.Errors is { Count: > 0 } ? response.Errors[0] : "No se pudo completar el registro.";
            throw new ApiClientException(message, response?.Errors);
        }

        return response.Data.Registration.Id;
    }
}

public record PublicFestivalSlugDto(Guid? ActiveEditionId, bool HasAccommodation, string? TermsUrl, string? FaviconUrl);

public record PublicFestivalBrandingDto(string Name, string? FaviconUrl, string? LogoUrl);

public record PublicEmailLayoutDto(string HeaderHtml, string FooterHtml);



