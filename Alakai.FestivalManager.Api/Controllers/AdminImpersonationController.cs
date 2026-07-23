using Alakai.FestivalManager.Application.Features.Auth.Services;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/admin/impersonation")]
[Authorize(Roles = "SuperAdmin")]
public class AdminImpersonationController : ControllerBase
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AdminImpersonationController(IRegistrationRepository registrationRepository, IUserRepository userRepository, IJwtService jwtService)
    {
        _registrationRepository = registrationRepository;
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    [HttpPost("by-registration/{registrationId:guid}")]
    public async Task<IActionResult> ImpersonateByRegistration(Guid registrationId, CancellationToken cancellationToken)
    {
        Registration? registration = await _registrationRepository.GetByIdAsync(registrationId, cancellationToken);

        if (registration is null)
        {
            return NotFound(new { error = "Registration not found." });
        }

        User? user = await _userRepository.GetByIdAsync(registration.UserId, cancellationToken);

        if (user is null)
        {
            return NotFound(new { error = "User not found for this registration." });
        }

        string accessToken = _jwtService.GenerateAccessToken(user);

        return Ok(new { accessToken });
    }
}