namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/emails")]
public class EmailsController : ControllerBase
{
    private readonly IEmailNotificationService _emailNotificationService;

    public EmailsController(IEmailNotificationService emailNotificationService)
    {
        _emailNotificationService = emailNotificationService;
    }

    [HttpPost("registrations/{registrationId:guid}/{templateKey}/send")]
    public async Task<ActionResult<ApiResponse<EmailLogDto>>> SendRegistrationEmail(Guid registrationId, EmailTemplateKey templateKey, CancellationToken cancellationToken)
    {
        EmailLogDto? emailLog = await _emailNotificationService.CreateAndSendEmailAsync(templateKey, registrationId, cancellationToken);

        if (emailLog is null)
        {
            return BadRequest(new ApiResponse<EmailLogDto>
            {
                Success = false,
                Message = "Email could not be created or sent.",
                Data = null,
                Errors = ["Template or registration not found."]
            });
        }

        if (emailLog.Status == EmailLogStatus.Failed)
        {
            return BadRequest(new ApiResponse<EmailLogDto>
            {
                Success = false,
                Message = "Email could not be sent.",
                Data = emailLog,
                Errors = [emailLog.ErrorMessage ?? "Email sending failed."]
            });
        }

        return Ok(new ApiResponse<EmailLogDto>
        {
            Success = true,
            Message = "Email sent successfully.",
            Data = emailLog,
            Errors = []
        });
    }
}