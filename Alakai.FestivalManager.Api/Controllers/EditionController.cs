namespace Alakai.FestivalManager.Api.Controllers;

public class UploadEditionScheduleForm
{
    public IFormFile File { get; set; } = default!;
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Production")]
public class EditionsController : ControllerBase
{
    private readonly IEditionService _editionService;
    private readonly IMapper _mapper;

    private static readonly HashSet<string> AllowedScheduleContentTypes = new(StringComparer.OrdinalIgnoreCase) { "application/pdf" };
    private const long MaxScheduleFileSizeBytes = 20 * 1024 * 1024; // 20 MB

    public EditionsController(IEditionService editionService, IMapper mapper)
    {
        _editionService = editionService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEditionRequest request, CancellationToken cancellationToken)
    {
        CreateEditionCommand command = _mapper.Map<CreateEditionCommand>(request);

        ApiResponse<CreateEditionResponse> response = await _editionService.CreateAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Edition.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetEditionByIdResponse> response = await _editionService.GetByIdAsync(id, cancellationToken);

        return Ok(response);
    }

    [HttpGet("by-festival/{festivalId:guid}")]
    public async Task<IActionResult> GetByFestivalId(Guid festivalId, CancellationToken cancellationToken)
    {
        ApiResponse<GetEditionsResponse> response = await _editionService.GetByFestivalIdAsync(festivalId, cancellationToken);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetEditionsResponse> response = await _editionService.GetAllAsync(cancellationToken);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEditionRequest request, CancellationToken cancellationToken)
    {
        UpdateEditionCommand command = _mapper.Map<UpdateEditionCommand>(request);
        command.Id = id;

        ApiResponse<UpdateEditionResponse> response = await _editionService.UpdateAsync(command, cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteEditionResponse> response = await _editionService.DeleteAsync(id, cancellationToken);

        return Ok(response);
    }

    [HttpPost("{id:guid}/reset-early-bird-usage")]
    public async Task<IActionResult> ResetEarlyBirdUsage(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<ResetEarlyBirdUsageResponse> response = await _editionService.ResetEarlyBirdUsageAsync(id, cancellationToken);

        return Ok(response);
    }

    [HttpPost("{id:guid}/schedule")]
    [RequestSizeLimit(MaxScheduleFileSizeBytes)]
    public async Task<IActionResult> SetSchedule(Guid id, [FromForm] UploadEditionScheduleForm form, CancellationToken cancellationToken)
    {
        IFormFile file = form.File;

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "No file was provided." });
        }

        if (file.Length > MaxScheduleFileSizeBytes)
        {
            return BadRequest(new { error = "File exceeds the maximum allowed size of 20 MB." });
        }

        if (!AllowedScheduleContentTypes.Contains(file.ContentType))
        {
            return BadRequest(new { error = "Only PDF files are allowed." });
        }

        using Stream stream = file.OpenReadStream();
        ApiResponse<SetEditionScheduleResponse> response = await _editionService.SetScheduleAsync(id, stream, file.FileName, cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{id:guid}/schedule")]
    public async Task<IActionResult> RemoveSchedule(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<RemoveEditionScheduleResponse> response = await _editionService.RemoveScheduleAsync(id, cancellationToken);

        return Ok(response);
    }
}