namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Services;

public interface IEmailTemplateService
{
    Task<ApiResponse<CreateEmailTemplateResponse>> CreateAsync(CreateEmailTemplateCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEmailTemplateByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEmailTemplatesResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetEmailTemplatesByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateEmailTemplateResponse>> UpdateAsync(UpdateEmailTemplateCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteEmailTemplateResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
