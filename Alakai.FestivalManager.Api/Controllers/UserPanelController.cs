using Alakai.FestivalManager.Application.Features.UserPanel.Contracts.Requests;

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
    public async Task<ActionResult<ApiResponse<GetUserPanelDashboardResponse>>> GetDashboard(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetUserPanelDashboardResponse> response = await _userPanelService.GetDashboardAsync(userId, cancellationToken);

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
}