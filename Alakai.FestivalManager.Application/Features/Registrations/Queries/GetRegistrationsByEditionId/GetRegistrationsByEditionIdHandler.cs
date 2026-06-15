using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;

namespace Alakai.FestivalManager.Application.Features.Registrations.Queries.GetRegistrationsByEditionId;

public class GetRegistrationsByEditionIdHandler
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IMapper _mapper;

    public GetRegistrationsByEditionIdHandler(IRegistrationRepository registrationRepository, IMapper mapper)
    {
        _registrationRepository = registrationRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<RegistrationDto>> HandleAsync(GetRegistrationsByEditionIdQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Alakai.FestivalManager.Domain.Entities.Registration> registrations = await _registrationRepository.GetByEditionIdAsync(query.EditionId, cancellationToken);
        IReadOnlyList<RegistrationDto> dtos = _mapper.Map<IReadOnlyList<RegistrationDto>>(registrations);
        return dtos;
    }
}
