namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/discount-codes")]
public class DiscountCodesController : ControllerBase
{
    private readonly IDiscountCodeService _discountCodeService;
    private readonly IMapper _mapper;

    public DiscountCodesController(IDiscountCodeService discountCodeService, IMapper mapper)
    {
        _discountCodeService = discountCodeService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetDiscountCodesResponse>>> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetDiscountCodesResponse> response = await _discountCodeService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetDiscountCodeByIdResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetDiscountCodeByIdResponse> response = await _discountCodeService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<ActionResult<ApiResponse<GetDiscountCodesByEditionIdResponse>>> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetDiscountCodesByEditionIdResponse> response = await _discountCodeService.GetByEditionIdAsync(editionId, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateDiscountCodeResponse>>> Create([FromBody] CreateDiscountCodeRequest request, CancellationToken cancellationToken)
    {
        CreateDiscountCodeCommand command = _mapper.Map<CreateDiscountCodeCommand>(request);
        ApiResponse<CreateDiscountCodeResponse> response = await _discountCodeService.CreateAsync(command, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UpdateDiscountCodeResponse>>> Update(Guid id, [FromBody] UpdateDiscountCodeRequest request, CancellationToken cancellationToken)
    {
        UpdateDiscountCodeCommand command = _mapper.Map<UpdateDiscountCodeCommand>(request);
        ApiResponse<UpdateDiscountCodeResponse> response = await _discountCodeService.UpdateAsync(id, command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteDiscountCodeResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteDiscountCodeResponse> response = await _discountCodeService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}
