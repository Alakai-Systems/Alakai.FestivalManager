namespace Alakai.FestivalManager.Application.Features.EmailLayouts.Contracts.Responses;

public class GetEmailLayoutResponse
{
    public EmailLayoutDto EmailLayout { get; set; } = new();
}