namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Services;

public interface IDiscountCodeService
{
    Task<ApiResponse<CreateDiscountCodeResponse>> CreateAsync(CreateDiscountCodeCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetDiscountCodeByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetDiscountCodesResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetDiscountCodesByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateDiscountCodeResponse>> UpdateAsync(Guid id, UpdateDiscountCodeCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteDiscountCodeResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
