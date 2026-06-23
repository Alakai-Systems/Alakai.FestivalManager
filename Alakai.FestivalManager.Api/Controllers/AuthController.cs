namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public AuthController(IAuthService authService, IMapper mapper)
    {
        _authService = authService;
        _mapper = mapper;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        LoginCommand command = _mapper.Map<LoginCommand>(request);
        ApiResponse<LoginResponse> response = await _authService.LoginAsync(command, cancellationToken);

        if (!response.Success)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        RefreshTokenCommand command = _mapper.Map<RefreshTokenCommand>(request);
        ApiResponse<RefreshTokenResponse> response = await _authService.RefreshTokenAsync(command, cancellationToken);

        if (!response.Success)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<ForgotPasswordResponse>>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        ForgotPasswordCommand command = _mapper.Map<ForgotPasswordCommand>(request);
        ApiResponse<ForgotPasswordResponse> response = await _authService.ForgotPasswordAsync(command, cancellationToken);

        return Ok(response);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<ResetPasswordResponse>>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        ResetPasswordCommand command = _mapper.Map<ResetPasswordCommand>(request);
        ApiResponse<ResetPasswordResponse> response = await _authService.ResetPasswordAsync(command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<ChangePasswordResponse>>> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ChangePasswordCommand command = _mapper.Map<ChangePasswordCommand>(request);
        command.UserId = userId;

        ApiResponse<ChangePasswordResponse> response = await _authService.ChangePasswordAsync(command, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<GetCurrentUserResponse>>> Me(CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        ApiResponse<GetCurrentUserResponse> response = await _authService.GetCurrentUserAsync(new GetCurrentUserQuery { UserId = userId }, cancellationToken);

        if (!response.Success)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<LogoutResponse>>> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
    {
        ApiResponse<LogoutResponse> response = await _authService.LogoutAsync(command, cancellationToken);

        return Ok(response);
    }
}
