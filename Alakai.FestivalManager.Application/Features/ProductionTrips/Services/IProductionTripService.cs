namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Services;

public interface IProductionTripService
{
    Task<ApiResponse<CreateTripResponse>> CreateAsync(CreateTripCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetTripResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetTripsResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateTripResponse>> UpdateAsync(UpdateTripCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteTripResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}