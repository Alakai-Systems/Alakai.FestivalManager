using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Festivals.Contracts.Responses;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
    public async Task<IActionResult> Create([FromBody] CreateFestivalCommand command, CancellationToken cancellationToken)
    {
        ApiResponse<CreateFestivalResponse> response = await _festivalService.CreateAsync(command, cancellationToken);

        return CreatedAtAction(nameof(Create), response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetFestivalByIdResponse> response = await _festivalService.GetByIdAsync(id, cancellationToken);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetFestivalsResponse> response = await _festivalService.GetAllAsync(cancellationToken);

        return Ok(response);
    }
}
