using Alakai.FestivalManager.Application.Features.Payments.Commands.ProcessRedsysReturn;

namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("session")]
    public async Task<IActionResult> CreateSession([FromBody] CreatePaymentSessionCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _paymentService.CreatePaymentSessionAsync(command, command.UrlOk, command.UrlKo, cancellationToken));
    }

    [HttpPost("redsys/confirm-return")]
    public async Task<IActionResult> ConfirmReturn([FromBody] ConfirmRedsysReturnCommand command, CancellationToken cancellationToken)
    {
        bool processed = await _paymentService.ProcessRedsysNotificationAsync(command.MerchantParameters, command.Signature, cancellationToken);

        return Ok(new ApiResponse<bool> { Success = processed, Data = processed, Errors = processed ? [] : ["The payment confirmation could not be validated."], Message = processed ? "Payment confirmed" : "Payment not confirmed" });
    }

    [HttpPost("redsys/process-return")]
    public async Task<IActionResult> ProcessReturn([FromBody] ProcessRedsysReturnCommand command, CancellationToken cancellationToken)
    {
        bool processed = await _paymentService.ProcessRedsysReturnAsync(command.MerchantParameters, cancellationToken);

        return Ok(new ApiResponse<bool> { Success = true, Data = processed, Errors = [], Message = processed ? "Payment confirmed" : "Payment not approved" });
    }

    [AllowAnonymous]
    [HttpPost("redsys/notification")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> RedsysNotification([FromForm(Name = "Ds_MerchantParameters")] string merchantParameters, [FromForm(Name = "Ds_Signature")] string signature, CancellationToken cancellationToken)
    {
        await _paymentService.ProcessRedsysNotificationAsync(merchantParameters, signature, cancellationToken);

        // Redsys only requires an HTTP 200 acknowledgment.
        return Ok();
    }
}
