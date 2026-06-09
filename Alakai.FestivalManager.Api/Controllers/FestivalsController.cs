namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
}
