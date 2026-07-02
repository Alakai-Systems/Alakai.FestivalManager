using Alakai.FestivalManager.Application.Features.EmailLayouts.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.EmailLayouts.Contracts.Responses;

public class CreateEmailLayoutResponse
{
    public EmailLayoutDto EmailLayout { get; set; } = new();
}