namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Responses;

public class GetEmailLogsByEditionIdResponse
{
    public IReadOnlyList<EmailLogDto> EmailLogs { get; set; } = [];
}
