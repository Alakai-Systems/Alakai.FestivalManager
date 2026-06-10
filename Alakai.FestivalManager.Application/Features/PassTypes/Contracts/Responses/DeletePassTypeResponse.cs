namespace Alakai.FestivalManager.Application.Features.PassTypes.Contracts.Responses;

public class DeletePassTypeResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}