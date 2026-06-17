namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Responses;

public class GetEmailLogByIdResponse
{
    public EmailLogDto EmailLog { get; set; } = default!;
}
