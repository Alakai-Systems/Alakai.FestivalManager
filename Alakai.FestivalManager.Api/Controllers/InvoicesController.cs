namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize(Roles = "SuperAdmin,Admin")]
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

    [HttpGet("zip")]
    public async Task<IActionResult> DownloadZip([FromQuery] Guid editionId, CancellationToken cancellationToken)
    {
        if (editionId == Guid.Empty)
        {
            return BadRequest("editionId is required.");
        }

        byte[] bytes = await _invoiceService.GetInvoicesZipForEditionAsync(editionId, cancellationToken);
        return File(bytes, "application/zip", $"invoices-{editionId}.zip");
    }

    [HttpPost("bulk-create")]
    public async Task<IActionResult> BulkCreate([FromQuery] Guid editionId, CancellationToken cancellationToken)
    {
        if (editionId == Guid.Empty)
        {
            return BadRequest("editionId is required.");
        }

        byte[] bytes = await _invoiceService.BulkCreateAndZipInvoicesForEditionAsync(editionId, cancellationToken);
        return File(bytes, "application/zip", $"invoices-{editionId}.zip");
    }
}