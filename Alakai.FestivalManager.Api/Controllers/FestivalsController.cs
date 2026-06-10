namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/festivals")]
public class FestivalsController : ControllerBase
{
    private readonly IFestivalService _festivalService;
    private readonly IMapper _mapper;

    public FestivalsController(IFestivalService festivalService, IMapper mapper)
    {
        _festivalService = festivalService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFestivalRequest request, CancellationToken cancellationToken)
    {
        CreateFestivalCommand command = _mapper.Map<CreateFestivalCommand>(request);
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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFestivalRequest request, CancellationToken cancellationToken)
    {
        UpdateFestivalCommand? command = _mapper.Map<UpdateFestivalCommand>(request);
        command.Id = id;

        ApiResponse<UpdateFestivalResponse> response = await _festivalService.UpdateAsync(command, cancellationToken);

        return Ok(response);
    }
}
