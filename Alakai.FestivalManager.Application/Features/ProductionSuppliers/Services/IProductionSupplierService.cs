namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Services;

public interface IProductionSupplierService
{
    Task<ApiResponse<CreateProductionSupplierResponse>> CreateAsync(CreateProductionSupplierCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetProductionSupplierByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetProductionSuppliersResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetProductionSuppliersResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateProductionSupplierResponse>> UpdateAsync(UpdateProductionSupplierCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteProductionSupplierResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}