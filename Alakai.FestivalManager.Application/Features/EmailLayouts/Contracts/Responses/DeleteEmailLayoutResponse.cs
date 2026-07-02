namespace Alakai.FestivalManager.Application.Features.EmailLayouts.Contracts.Responses;

public class DeleteEmailLayoutResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}