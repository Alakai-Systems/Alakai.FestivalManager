namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Services;

public interface IProductionAccommodationService
{
    Task<ApiResponse<GetProductionAccommodationsResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateProductionAccommodationResponse>> CreateAsync(CreateProductionAccommodationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetProductionAccommodationByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetProductionAccommodationsResponse>> GetByZoneIdAsync(Guid zoneId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateProductionAccommodationResponse>> UpdateAsync(UpdateProductionAccommodationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteProductionAccommodationResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}