using Alakai.FestivalManager.Application.Features.Competitions.Contracts.Responses;

namespace Alakai.FestivalManager.Application.Features.Competitions.Services;

public interface ICompetitionService
{
    Task<ApiResponse<CreateCompetitionResponse>> CreateAsync(CreateCompetitionCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCompetitionByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCompetitionsResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCompetitionsByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateCompetitionResponse>> UpdateAsync(Guid id, UpdateCompetitionCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteCompetitionResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
