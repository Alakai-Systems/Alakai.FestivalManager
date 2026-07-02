using Alakai.FestivalManager.Admin.Contracts.EmailLayouts.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.EmailLayouts.Responses;

public class CreateEmailLayoutResponse
{
    public EmailLayoutDto EmailLayout { get; set; } = new();
}