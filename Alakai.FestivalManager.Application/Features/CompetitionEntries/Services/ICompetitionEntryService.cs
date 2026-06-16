using Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.Responses;

namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Services;

public interface ICompetitionEntryService
{
    Task<ApiResponse<CreateCompetitionEntryResponse>> CreateAsync(CreateCompetitionEntryCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCompetitionEntryByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCompetitionEntriesResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCompetitionEntriesByCompetitionIdResponse>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetCompetitionEntriesByRegistrationIdResponse>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateCompetitionEntryResponse>> UpdateAsync(Guid id, UpdateCompetitionEntryCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteCompetitionEntryResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
