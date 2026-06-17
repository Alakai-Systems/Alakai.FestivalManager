namespace Alakai.FestivalManager.Application.Features.EmailLogs.Services;

public interface IEmailLogService
{
    Task<ApiResponse<CreateEmailLogResponse>> CreateAsync(CreateEmailLogCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEmailLogByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEmailLogsResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEmailLogsByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEmailLogsByRegistrationIdResponse>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEmailLogsByUserIdResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateEmailLogResponse>> UpdateAsync(Guid id, UpdateEmailLogCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteEmailLogResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
