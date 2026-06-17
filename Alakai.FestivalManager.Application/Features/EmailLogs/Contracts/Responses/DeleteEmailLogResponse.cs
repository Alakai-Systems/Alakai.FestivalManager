namespace Alakai.FestivalManager.Application.Features.EmailLogs.Contracts.Responses;

public class DeleteEmailLogResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}
