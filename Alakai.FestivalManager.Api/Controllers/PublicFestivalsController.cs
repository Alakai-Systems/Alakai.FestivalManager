using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public/festivals")]
public class PublicFestivalsController : ControllerBase
{
    private readonly IFestivalRepository _festivalRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IEmailLayoutRepository _emailLayoutRepository;
    private readonly IConfiguration _configuration;

    public PublicFestivalsController(IFestivalRepository festivalRepository, IEditionRepository editionRepository,
        IEmailLayoutRepository emailLayoutRepository, IConfiguration configuration)
    {
        _festivalRepository = festivalRepository;
        _editionRepository = editionRepository;
        _emailLayoutRepository = emailLayoutRepository;
        _configuration = configuration;
    }

    [HttpGet("email-layout/{editionId:guid}")]
    public async Task<IActionResult> GetEmailLayout(Guid editionId, CancellationToken cancellationToken)
    {
        EmailLayout? layout = await _emailLayoutRepository.GetForEditionAsync(editionId, cancellationToken);

        return Ok(new
        {
            headerHtml = layout?.HeaderHtml ?? string.Empty,
            footerHtml = layout?.FooterHtml ?? string.Empty
        });
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        Festival? festival = await _festivalRepository.GetBySlugAsync(slug, cancellationToken);

        if (festival is null)
        {
            return NotFound();
        }

        IReadOnlyList<Edition> editions = await _editionRepository.GetByFestivalIdAsync(festival.Id, cancellationToken);

        Edition? active = editions
            .Where(e => e.IsActive)
            .OrderByDescending(e => e.Year)
            .FirstOrDefault();

        bool hasAccommodation = (festival.EnabledModules & FestivalModule.Accommodation) != 0;

        return Ok(new
        {
            ActiveEditionId = active?.Id,
            HasAccommodation = hasAccommodation,
            TermsUrl = festival.TermsUrl,
            FaviconUrl = festival.FaviconUrl
        });
    }

    [HttpGet("by-domain/{domain}")]
    public async Task<IActionResult> GetByDomain(string domain, CancellationToken cancellationToken)
    {
        Festival? festival = await _festivalRepository.GetByCustomDomainAsync(domain, cancellationToken);

        if (festival is not null)
        {
            return Ok(new
            {
                Name = festival.Name,
                FaviconUrl = festival.FaviconUrl,
                LogoUrl = festival.LogoUrl
            });
        }

        string platformName = _configuration["PlatformBranding:Name"] ?? "Alakai Festival Manager";
        string? platformLogoUrl = _configuration["PlatformBranding:LogoUrl"];
        string? platformFaviconUrl = _configuration["PlatformBranding:FaviconUrl"];

        return Ok(new
        {
            Name = platformName,
            FaviconUrl = string.IsNullOrWhiteSpace(platformFaviconUrl) ? null : platformFaviconUrl,
            LogoUrl = string.IsNullOrWhiteSpace(platformLogoUrl) ? null : platformLogoUrl
        });
    }
}
