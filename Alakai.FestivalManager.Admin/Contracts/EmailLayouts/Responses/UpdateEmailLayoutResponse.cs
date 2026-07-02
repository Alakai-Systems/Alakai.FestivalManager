using Alakai.FestivalManager.Admin.Contracts.EmailLayouts.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.EmailLayouts.Responses;

public class UpdateEmailLayoutResponse
{
    public EmailLayoutDto EmailLayout { get; set; } = new();
}