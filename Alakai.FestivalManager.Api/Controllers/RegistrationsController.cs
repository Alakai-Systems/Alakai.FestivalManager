using Alakai.FestivalManager.Application.Features.Registrations.Queries.GetRegistrationById;

namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly IMapper _mapper;

    public RegistrationsController(IRegistrationService registrationService, IMapper mapper)
    {
        _registrationService = registrationService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRegistrationRequest request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[DEBUG] PaymentPlan received: {request.PaymentPlan} ({(int)request.PaymentPlan})");
        CreateRegistrationCommand command = _mapper.Map<CreateRegistrationCommand>(request);
        Console.WriteLine($"[DEBUG] PaymentPlan in command: {command.PaymentPlan} ({(int)command.PaymentPlan})");
        

        ApiResponse <CreateRegistrationResponse> response = await _registrationService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Registration.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<GetRegistrationByIdResponse> response = await _registrationService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ApiResponse<GetRegistrationsResponse> response = await _registrationService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-userId/{userId:guid}")]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        ApiResponse<GetRegistrationByUserIdResponse> response = await _registrationService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<IActionResult> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetRegistrationsByEditionIdResponse> response = await _registrationService.GetByEditionIdAsync(editionId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRegistrationRequest request, CancellationToken cancellationToken)
    {

        UpdateRegistrationCommand command = _mapper.Map<UpdateRegistrationCommand>(request);

        ApiResponse<UpdateRegistrationResponse> response = await _registrationService.UpdateAsync(id, command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ApiResponse<DeleteRegistrationResponse> response = await _registrationService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }
}
