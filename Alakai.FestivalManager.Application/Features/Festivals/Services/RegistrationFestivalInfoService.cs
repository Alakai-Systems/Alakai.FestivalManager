using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Features.Festivals.Contracts.DTOs;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Festivals.Services;

public class RegistrationFestivalInfoService : IRegistrationFestivalInfoService
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IFestivalRepository _festivalRepository;

    public RegistrationFestivalInfoService(
        IRegistrationRepository registrationRepository,
        IEditionRepository editionRepository,
        IFestivalRepository festivalRepository)
    {
        _registrationRepository = registrationRepository;
        _editionRepository = editionRepository;
        _festivalRepository = festivalRepository;
    }

    public async Task<ApiResponse<RegistrationFestivalInfoDto>> GetForRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _registrationRepository.GetByIdAsync(registrationId, cancellationToken);

        if (registration is null)
        {
            throw new NotFoundException($"Registration with id '{registrationId}' was not found.");
        }

        Edition? edition = await _editionRepository.GetByIdAsync(registration.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{registration.EditionId}' was not found.");
        }

        Festival? festival = await _festivalRepository.GetByIdAsync(edition.FestivalId, cancellationToken);

        if (festival is null)
        {
            throw new NotFoundException($"Festival with id '{edition.FestivalId}' was not found.");
        }

        return new ApiResponse<RegistrationFestivalInfoDto>
        {
            Success = true,
            Data = new RegistrationFestivalInfoDto { EnabledModules = (int)festival.EnabledModules, TermsUrl = festival.TermsUrl },
            Errors = [],
            Message = "Festival info loaded."
        };
    }
}