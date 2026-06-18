namespace Alakai.FestivalManager.Admin.Contracts.EmailLogs.Responses;

public class GetEmailLogsByRegistrationIdResponse
{
    public IReadOnlyList<EmailLogDto> EmailLogs { get; set; } = [];
}
