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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Alakai.FestivalManager.Application.Features.Invoices.Commands.UpdateInvoice.UpdateInvoiceCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _invoiceService.UpdateAsync(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _invoiceService.DeleteAsync(id, cancellationToken));
    }
}