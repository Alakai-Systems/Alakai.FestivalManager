using Alakai.FestivalManager.Admin.Contracts.EmailLayouts.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.EmailLayouts.Responses;

public class GetEmailLayoutsResponse
{
    public IReadOnlyList<EmailLayoutDto> EmailLayouts { get; set; } = [];
}