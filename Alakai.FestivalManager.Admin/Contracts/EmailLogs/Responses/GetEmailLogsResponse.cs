namespace Alakai.FestivalManager.Admin.Contracts.EmailLogs.Responses;

public class GetEmailLogsResponse
{
    public IReadOnlyList<EmailLogDto> EmailLogs { get; set; } = [];
}
