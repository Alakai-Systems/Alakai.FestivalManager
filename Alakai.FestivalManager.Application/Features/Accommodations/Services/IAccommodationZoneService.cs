namespace Alakai.FestivalManager.Application.Features.Accommodations.Services;

public interface IAccommodationZoneService
{
    Task<ApiResponse<CreateAccommodationZoneResponse>> CreateAsync(CreateAccommodationZoneCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateAccommodationZoneResponse>> UpdateAsync(UpdateAccommodationZoneCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteAccommodationEntityResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}