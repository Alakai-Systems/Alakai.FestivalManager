namespace Alakai.FestivalManager.Admin.Contracts.EmailLogs.Responses;

public class GetEmailLogsByUserIdResponse
{
    public IReadOnlyList<EmailLogDto> EmailLogs { get; set; } = [];
}
