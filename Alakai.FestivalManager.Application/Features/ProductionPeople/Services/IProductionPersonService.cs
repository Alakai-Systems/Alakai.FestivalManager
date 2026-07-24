namespace Alakai.FestivalManager.Application.Features.ProductionPeople.Services;

public interface IProductionPersonService
{
    Task<ApiResponse<CreateProductionPersonResponse>> CreateAsync(CreateProductionPersonCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetProductionPersonByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetProductionPeopleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetProductionPeopleResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateProductionPersonResponse>> UpdateAsync(UpdateProductionPersonCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteProductionPersonResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}