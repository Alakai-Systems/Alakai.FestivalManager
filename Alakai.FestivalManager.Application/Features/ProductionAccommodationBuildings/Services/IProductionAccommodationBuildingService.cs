namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationBuildings.Services;

public interface IProductionAccommodationBuildingService
{
    Task<ApiResponse<CreateProductionAccommodationBuildingResponse>> CreateAsync(CreateProductionAccommodationBuildingCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetProductionAccommodationBuildingByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetProductionAccommodationBuildingsResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetProductionAccommodationBuildingsResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateProductionAccommodationBuildingResponse>> UpdateAsync(UpdateProductionAccommodationBuildingCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteProductionAccommodationBuildingResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}