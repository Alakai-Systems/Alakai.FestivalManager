using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Festivals.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Festivals.Services;

public interface IRegistrationFestivalInfoService
{
    Task<ApiResponse<RegistrationFestivalInfoDto>> GetForRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default);
}