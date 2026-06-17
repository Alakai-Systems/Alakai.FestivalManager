namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Responses;

public class GetEmailLogsResponse
{
    public IReadOnlyList<EmailLogDto> EmailLogs { get; set; } = [];
}
