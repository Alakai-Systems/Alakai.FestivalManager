using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Buses.Commands;
using Alakai.FestivalManager.Application.Features.Buses.Contracts.Responses;

namespace Alakai.FestivalManager.Application.Features.Buses.Services;

public interface IBusReservationService
{
    Task<ApiResponse<GetBusReservationsResponse>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusReservationsResponse>> GetByBusIdAsync(Guid busId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusReservationsResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateBusReservationResponse>> CreateAsync(CreateBusReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusReservationsResponse>> CreateManyAsync(CreateBusReservationsCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateBusReservationResponse>> UpdateAsync(UpdateBusReservationCommand command, bool isAdmin, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteBusReservationResponse>> DeleteAsync(Guid id, Guid requestingRegistrationId, bool isAdmin, CancellationToken cancellationToken = default);
}