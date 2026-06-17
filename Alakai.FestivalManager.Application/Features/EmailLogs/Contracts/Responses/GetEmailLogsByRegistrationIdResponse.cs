namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Responses;

public class GetEmailLogsByRegistrationIdResponse
{
    public IReadOnlyList<EmailLogDto> EmailLogs { get; set; } = [];
}
