namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Services;

public interface IProductionReservationService
{
    Task<ApiResponse<GetReservationsResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateReservationResponse>> CreateAsync(CreateReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetReservationResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetReservationsResponse>> GetByBuildingIdAsync(Guid buildingId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateReservationResponse>> UpdateAsync(UpdateReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteReservationResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}