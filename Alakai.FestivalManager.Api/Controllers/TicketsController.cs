namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpPost("checkin")]
    public async Task<ActionResult<ApiResponse<TicketCheckInResultDto>>> CheckIn([FromBody] CheckInTicketRequest request, CancellationToken cancellationToken)
    {
        TicketCheckInResultDto? result = await _ticketService.CheckInAsync(request.Token, cancellationToken);

        if (result is null)
        {
            return BadRequest(new ApiResponse<TicketCheckInResultDto>
            {
                Success = false,
                Message = "Invalid or unrecognised QR code.",
                Data = null,
                Errors = ["The QR code is not valid or does not correspond to a registration."]
            });
        }

        return Ok(new ApiResponse<TicketCheckInResultDto>
        {
            Success = true,
            Message = result.AlreadyCheckedIn ? "This ticket was already checked in." : "Check-in confirmed.",
            Data = result,
            Errors = []
        });
    }
}