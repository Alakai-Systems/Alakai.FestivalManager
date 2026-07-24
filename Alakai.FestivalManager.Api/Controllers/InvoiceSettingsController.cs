namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/invoice-settings")]
public class InvoiceSettingsController : ControllerBase
{
    private readonly IInvoiceSettingsService _invoiceSettingsService;

    public InvoiceSettingsController(IInvoiceSettingsService invoiceSettingsService)
    {
        _invoiceSettingsService = invoiceSettingsService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetInvoiceSettingsResponse>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _invoiceSettingsService.GetAsync(cancellationToken));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<UpdateInvoiceSettingsResponse>>> Update([FromBody] UpdateInvoiceSettingsCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _invoiceSettingsService.UpdateAsync(command, cancellationToken));
    }
}