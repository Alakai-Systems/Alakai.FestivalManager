namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/festivals")]
public class FestivalsController : ControllerBase
{
    private readonly IFestivalService _festivalService;

    public FestivalsController(IFestivalService festivalService)
    {
        _festivalService = festivalService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateFestivalCommand command,
        CancellationToken cancellationToken)
    {
        FestivalDto result = await _festivalService.CreateAsync(command, cancellationToken);

        return CreatedAtAction(
            nameof(Create),
            new { id = result.Id },
            result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        FestivalDto? festival = await _festivalService.GetByIdAsync(id, cancellationToken);

        if (festival is null)
        {
            return NotFound();
        }

        return Ok(festival);
    }
}
