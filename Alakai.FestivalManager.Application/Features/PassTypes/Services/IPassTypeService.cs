namespace Alakai.FestivalManager.Application.Features.PassTypes.Services;

public interface IPassTypeService
{
    Task<ApiResponse<CreatePassTypeResponse>> CreateAsync(CreatePassTypeCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetPassTypeByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetPassTypesResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetPassTypesResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdatePassTypeResponse>> UpdateAsync(UpdatePassTypeCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeletePassTypeResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}