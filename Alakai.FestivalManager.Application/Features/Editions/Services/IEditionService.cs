namespace Alakai.FestivalManager.Application.Features.Editions.Services;

public interface IEditionService
{
    Task<ApiResponse<CreateEditionResponse>> CreateAsync(CreateEditionCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEditionByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEditionsResponse>> GetByFestivalIdAsync(Guid festivalId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEditionsResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateEditionResponse>> UpdateAsync(UpdateEditionCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteEditionResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
