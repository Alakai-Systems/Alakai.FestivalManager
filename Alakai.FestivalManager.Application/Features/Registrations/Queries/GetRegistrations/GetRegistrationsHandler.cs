using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;

namespace Alakai.FestivalManager.Application.Features.Registrations.Queries.GetRegistrations;

public class GetRegistrationsHandler
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IMapper _mapper;

    public GetRegistrationsHandler(IRegistrationRepository registrationRepository, IMapper mapper)
    {
        _registrationRepository = registrationRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<RegistrationDto>> HandleAsync(GetRegistrationsQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Alakai.FestivalManager.Domain.Entities.Registration> registrations = await _registrationRepository.GetAllAsync(cancellationToken);
        IReadOnlyList<RegistrationDto> dtos = _mapper.Map<IReadOnlyList<RegistrationDto>>(registrations);
        return dtos;
    }
}
