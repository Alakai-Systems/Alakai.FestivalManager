namespace Alakai.FestivalManager.Application.Features.Payments.Services;

public interface IPaymentService
{
    Task<ApiResponse<RedsysPaymentFormDto>> CreatePaymentSessionAsync(CreatePaymentSessionCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<RedsysPaymentFormDto>> CreatePaymentSessionAsync(CreatePaymentSessionCommand command, string? urlOk, string? urlKo, CancellationToken cancellationToken = default);
    Task<bool> ProcessRedsysNotificationAsync(string merchantParameters, string signature, CancellationToken cancellationToken = default);
    Task<bool> ProcessRedsysReturnAsync(string merchantParameters, CancellationToken cancellationToken = default);
}
