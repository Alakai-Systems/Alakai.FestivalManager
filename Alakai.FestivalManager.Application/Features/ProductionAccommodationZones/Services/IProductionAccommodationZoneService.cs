namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Services;

public interface IProductionAccommodationZoneService
{
    Task<ApiResponse<GetProductionAccommodationZonesResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateProductionAccommodationZoneResponse>> CreateAsync(CreateProductionAccommodationZoneCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetProductionAccommodationZoneByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetProductionAccommodationZonesResponse>> GetByBuildingIdAsync(Guid buildingId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateProductionAccommodationZoneResponse>> UpdateAsync(UpdateProductionAccommodationZoneCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteProductionAccommodationZoneResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}