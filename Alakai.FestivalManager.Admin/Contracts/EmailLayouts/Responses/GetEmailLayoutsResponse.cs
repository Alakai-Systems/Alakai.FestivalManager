
namespace Alakai.FestivalManager.Admin.Contracts.EmailLayouts.Responses;

public class GetEmailLayoutsResponse
{
    public IReadOnlyList<EmailLayoutDto> EmailLayouts { get; set; } = [];
}