
using Alakai.FestivalManager.Application.Features.Registrations.Services;
using Alakai.FestivalManager.Application.Features.Registrations.Contracts.Requests;
using Alakai.FestivalManager.Application.Features.Registrations.Commands.CreateRegistration;
using Alakai.FestivalManager.Application.Features.Registrations.Contracts.Responses;
using Alakai.FestivalManager.Application.Features.Registrations.Commands.UpdateRegistration;


namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationsController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRegistrationRequest request, CancellationToken cancellationToken)
    {
        CreateRegistrationCommand command = new()
        {
            EditionId = request.EditionId,
            PassTypeId = request.PassTypeId,
            LevelId = request.LevelId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Country = request.Country,
            City = request.City,
            DanceRole = request.DanceRole,
            PartnerEmail = request.PartnerEmail,
            BasePrice = request.BasePrice,
            DiscountAmount = request.DiscountAmount,
            FinalPrice = request.FinalPrice,
            DiscountCode = request.DiscountCode,
            Notes = request.Notes,
            InternalNotes = request.InternalNotes
        };

        ApiResponse<CreateRegistrationResponse> response = await _registrationService.CreateAsync(command, cancellationToken);
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

    [HttpGet("by-edition/{editionId:guid}")]
    public async Task<IActionResult> GetByEditionId(Guid editionId, CancellationToken cancellationToken)
    {
        ApiResponse<GetRegistrationsByEditionIdResponse> response = await _registrationService.GetByEditionIdAsync(editionId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRegistrationRequest request, CancellationToken cancellationToken)
    {
        UpdateRegistrationCommand command = new()
        {
            Id = id,
            EditionId = request.EditionId,
            PassTypeId = request.PassTypeId,
            LevelId = request.LevelId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Country = request.Country,
            City = request.City,
            DanceRole = request.DanceRole,
            PartnerEmail = request.PartnerEmail,
            PartnerRegistrationId = request.PartnerRegistrationId,
            Status = request.Status,
            PaymentStatus = request.PaymentStatus,
            BasePrice = request.BasePrice,
            DiscountAmount = request.DiscountAmount,
            FinalPrice = request.FinalPrice,
            DiscountCode = request.DiscountCode,
            PaymentReference = request.PaymentReference,
            PaidAt = request.PaidAt,
            Notes = request.Notes,
            InternalNotes = request.InternalNotes,
            CancelledAt = request.CancelledAt,
            IsActive = request.IsActive
        };

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
