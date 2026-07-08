namespace Alakai.FestivalManager.Application.Features.Accommodations.Services;

public interface IAccommodationBuildingService
{
    Task<ApiResponse<GetAccommodationBuildingsResponse>> GetAllAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationBuildingResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationBuildingsResponse>> GetAvailableForRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateAccommodationBuildingResponse>> CreateAsync(CreateAccommodationBuildingCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateAccommodationBuildingResponse>> UpdateAsync(UpdateAccommodationBuildingCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteAccommodationEntityResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}