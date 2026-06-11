namespace Alakai.FestivalManager.Application.Features.Levels.Services;

public interface ILevelService
{
    Task<ApiResponse<CreateLevelResponse>> CreateAsync(CreateLevelCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetLevelByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetLevelsResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetLevelsResponse>> GetByPassTypeIdAsync(Guid passTypeId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateLevelResponse>> UpdateAsync(UpdateLevelCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteLevelResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
