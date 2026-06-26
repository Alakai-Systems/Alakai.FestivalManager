namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/emails")]
public class EmailsController : ControllerBase
{
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IEmailDispatcher _emailDispatcher;

    public EmailsController(IEmailNotificationService emailNotificationService, IEmailDispatcher emailDispatcher)
    {
        _emailNotificationService = emailNotificationService;
        _emailDispatcher = emailDispatcher;
    }

    [HttpPost("registrations/{registrationId:guid}/{templateKey}/send")]
    public async Task<ActionResult<ApiResponse<EmailLogDto>>> SendRegistrationEmail(Guid registrationId, EmailTemplateKey templateKey, CancellationToken cancellationToken)
    {
        EmailLogDto? emailLog = await _emailNotificationService.CreateEmailLogAsync(templateKey, registrationId, cancellationToken);

        if (emailLog is null)
        {
            return BadRequest(new ApiResponse<EmailLogDto>
            {
                Success = false,
                Message = "Email log could not be created.",
                Data = null,
                Errors = ["Template or registration not found."]
            });
        }

        bool sent = await _emailDispatcher.SendEmailLogAsync(emailLog.Id, cancellationToken);

        if (!sent)
        {
            return BadRequest(new ApiResponse<EmailLogDto>
            {
                Success = false,
                Message = "Email could not be sent.",
                Data = emailLog,
                Errors = ["Email sending failed."]
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
