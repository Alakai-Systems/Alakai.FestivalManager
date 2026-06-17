namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Responses;

public class GetEmailLogsByUserIdResponse
{
    public IReadOnlyList<EmailLogDto> EmailLogs { get; set; } = [];
}
