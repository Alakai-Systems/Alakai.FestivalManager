namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/invoice-templates")]
public class InvoiceTemplatesController : ControllerBase
{
    private readonly IInvoiceTemplateService _invoiceTemplateService;

    public InvoiceTemplatesController(IInvoiceTemplateService invoiceTemplateService)
    {
        _invoiceTemplateService = invoiceTemplateService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetInvoiceTemplatesResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _invoiceTemplateService.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateInvoiceTemplateResponse>>> Create([FromBody] CreateInvoiceTemplateCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _invoiceTemplateService.CreateAsync(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UpdateInvoiceTemplateResponse>>> Update(Guid id, [FromBody] UpdateInvoiceTemplateCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _invoiceTemplateService.UpdateAsync(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteInvoiceTemplateResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _invoiceTemplateService.DeleteAsync(id, cancellationToken));
    }
}