namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetUsersResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<GetUserByIdResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!IsSelfOrAdmin(id))
        {
            return Forbid();
        }

        return Ok(await _userService.GetByIdAsync(id, cancellationToken));
    }

    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<ApiResponse<GetUserByIdResponse>>> GetByEmail(string email, CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetByEmailAsync(email, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateUserResponse>>> Create([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _userService.CreateAsync(command, cancellationToken));
    }

    [HttpPost("admins")]
    public async Task<ActionResult<ApiResponse<CreateUserResponse>>> CreateAdmin([FromBody] CreateAdminUserCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _userService.CreateAdminAsync(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UpdateUserResponse>>> Update(Guid id, [FromBody] UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (!IsSelfOrAdmin(id))
        {
            return Forbid();
        }

        return Ok(await _userService.UpdateAsync(id, command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteUserResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _userService.DeleteAsync(id, cancellationToken));
    }

    private bool IsSelfOrAdmin(Guid id)
    {
        if (User.IsInRole("SuperAdmin") || User.IsInRole("Admin"))
        {
            return true;
        }

        string? currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(currentUserId, out Guid parsedId) && parsedId == id;
    }
}