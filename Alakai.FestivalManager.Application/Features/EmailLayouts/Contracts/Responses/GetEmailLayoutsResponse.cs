using Alakai.FestivalManager.Application.Features.EmailLayouts.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.EmailLayouts.Contracts.Responses;

public class GetEmailLayoutsResponse
{
    public IReadOnlyList<EmailLayoutDto> EmailLayouts { get; set; } = [];
}