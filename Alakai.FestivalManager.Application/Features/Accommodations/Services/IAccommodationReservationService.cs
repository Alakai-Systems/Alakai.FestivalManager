using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Accommodations.Commands;
using Alakai.FestivalManager.Application.Features.Accommodations.Contracts.Responses;

namespace Alakai.FestivalManager.Application.Features.Accommodations.Services;

public interface IAccommodationReservationService
{
    Task<ApiResponse<CreateAccommodationReservationResponse>> CreateAsync(CreateAccommodationReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationReservationsResponse>> GetByBuildingIdAsync(Guid buildingId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationReservationResponse>> GetByResponsibleRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateAccommodationReservationResponse>> UpdateAsync(UpdateAccommodationReservationCommand command, bool isAdmin, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteAccommodationReservationResponse>> DeleteAsync(Guid id, Guid requestingRegistrationId, bool isAdmin, CancellationToken cancellationToken = default);
}