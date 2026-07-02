namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetInvoicesResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _invoiceService.GetAllAsync(cancellationToken));
    }
}