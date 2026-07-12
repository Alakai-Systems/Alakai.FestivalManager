using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public/festivals")]
public class PublicFestivalsController : ControllerBase
{
    private readonly IFestivalRepository _festivalRepository;
    private readonly IEditionRepository _editionRepository;

    public PublicFestivalsController(IFestivalRepository festivalRepository, IEditionRepository editionRepository)
    {
        _festivalRepository = festivalRepository;
        _editionRepository = editionRepository;
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
            TermsUrl = festival.TermsUrl
        });
    }
}
