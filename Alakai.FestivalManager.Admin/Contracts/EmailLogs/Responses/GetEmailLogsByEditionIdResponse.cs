namespace Alakai.FestivalManager.Admin.Contracts.EmailLogs.Responses;

public class GetEmailLogsByEditionIdResponse
{
    public IReadOnlyList<EmailLogDto> EmailLogs { get; set; } = [];
}
