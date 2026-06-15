namespace Alakai.FestivalManager.Application.Features.Registrations.Services;

public interface IRegistrationService
{
    Task<ApiResponse<CreateRegistrationResponse>> CreateAsync(Alakai.FestivalManager.Application.Features.Registrations.Commands.CreateRegistration.CreateRegistrationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetRegistrationByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetRegistrationsResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetRegistrationsByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateRegistrationResponse>> UpdateAsync(Guid id, Alakai.FestivalManager.Application.Features.Registrations.Commands.UpdateRegistration.UpdateRegistrationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteRegistrationResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
