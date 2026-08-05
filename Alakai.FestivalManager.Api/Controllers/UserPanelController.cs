namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/user-panel")]
[Authorize]
public class UserPanelController : ControllerBase
{
    private readonly IUserPanelService _userPanelService;

    public UserPanelController(IUserPanelService userPanelService)
    {
        _userPanelService = userPanelService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> GetDashboard([FromQuery] string? domain, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.GetDashboardAsync(userId, domain, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> UpdateProfile([FromBody] UpdateUserPanelProfileRequest request, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.UpdateProfileAsync(userId, request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("competition-entries")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> CreateCompetitionEntry([FromBody] CreateCompetitionEntryRequest request, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.CreateCompetitionEntryAsync(userId, request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPut("competition-entries/{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> UpdateCompetitionEntry(Guid id, [FromBody] UpdateCompetitionEntryRequest request, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.UpdateCompetitionEntryAsync(userId, id, request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("competition-entries/{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> DeleteCompetitionEntry(Guid id, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.DeleteCompetitionEntryAsync(userId, id, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("invoices")]
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> CreateInvoice([FromBody] CreateUserPanelInvoiceRequest request, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.CreateInvoiceAsync(userId, request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("meal-preference")]
    public async Task<ActionResult<ApiResponse<GetMealPreferenceResponse>>> GetMealPreference(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetMealPreferenceResponse> response = await _userPanelService.GetMealPreferenceAsync(userId, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("meal-preference")]
    public async Task<ActionResult<ApiResponse<SaveMealPreferenceResponse>>> SaveMealPreference([FromBody] SaveMealPreferenceCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<SaveMealPreferenceResponse> response = await _userPanelService.SaveMealPreferenceAsync(userId, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("festival-modules")]
    public async Task<ActionResult<ApiResponse<RegistrationFestivalInfoDto>>> GetFestivalModules(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<RegistrationFestivalInfoDto> response = await _userPanelService.GetFestivalModulesAsync(userId, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("bus-reservations")]
    public async Task<ActionResult<ApiResponse<GetBusReservationsResponse>>> GetBusReservations(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetBusReservationsResponse> response = await _userPanelService.GetBusReservationsAsync(userId, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("available-buses")]
    public async Task<ActionResult<ApiResponse<GetBusesResponse>>> GetAvailableBuses(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetBusesResponse> response = await _userPanelService.GetAvailableBusesAsync(userId, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("bus-reservations")]
    public async Task<ActionResult<ApiResponse<GetBusReservationsResponse>>> CreateBusReservations([FromBody] CreateBusReservationsCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetBusReservationsResponse> response = await _userPanelService.CreateBusReservationsAsync(userId, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPut("bus-reservations/{id:guid}")]
    public async Task<ActionResult<ApiResponse<CreateBusReservationResponse>>> UpdateBusReservation(Guid id, [FromBody] UpdateBusReservationCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<CreateBusReservationResponse> response = await _userPanelService.UpdateBusReservationAsync(userId, id, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("bus-reservations/{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteBusReservationResponse>>> DeleteBusReservation(Guid id, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<DeleteBusReservationResponse> response = await _userPanelService.DeleteBusReservationAsync(userId, id, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("accommodation-reservation")]
    public async Task<ActionResult<ApiResponse<GetAccommodationReservationResponse>>> GetAccommodationReservation(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetAccommodationReservationResponse> response = await _userPanelService.GetAccommodationReservationAsync(userId, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("available-accommodations")]
    public async Task<ActionResult<ApiResponse<GetAccommodationBuildingsResponse>>> GetAvailableAccommodations(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetAccommodationBuildingsResponse> response = await _userPanelService.GetAvailableAccommodationsAsync(userId, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("accommodation-buildings/{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetAccommodationBuildingResponse>>> GetAccommodationBuilding(Guid id, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetAccommodationBuildingResponse> response = await _userPanelService.GetAccommodationBuildingAsync(id, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("accommodation-reservation")]
    public async Task<ActionResult<ApiResponse<CreateAccommodationReservationResponse>>> CreateAccommodationReservation([FromBody] CreateAccommodationReservationCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<CreateAccommodationReservationResponse> response = await _userPanelService.CreateAccommodationReservationAsync(userId, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPut("accommodation-reservation/{id:guid}")]
    public async Task<ActionResult<ApiResponse<CreateAccommodationReservationResponse>>> UpdateAccommodationReservation(Guid id, [FromBody] UpdateAccommodationReservationCommand command, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<CreateAccommodationReservationResponse> response = await _userPanelService.UpdateAccommodationReservationAsync(userId, id, command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("accommodation-reservation/{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteAccommodationReservationResponse>>> DeleteAccommodationReservation(Guid id, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<DeleteAccommodationReservationResponse> response = await _userPanelService.DeleteAccommodationReservationAsync(userId, id, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}