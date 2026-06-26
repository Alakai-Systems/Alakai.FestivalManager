namespace Alakai.FestivalManager.Application.Features.Registrations.Queries.GetRegistrationById;

public class GetRegistrationByUserIdHandler
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IMapper _mapper;

    public GetRegistrationByUserIdHandler(IRegistrationRepository registrationRepository, IMapper mapper)
    {
        _registrationRepository = registrationRepository;
        _mapper = mapper;
    }

    public async Task<RegistrationDto?> HandleAsync(GetRegistrationByUserIdQuery query, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _registrationRepository.GetByUserIdAsync(query.Id, cancellationToken);

        if (registration is null)
        {
            return null;
        }

        RegistrationDto dto = _mapper.Map<RegistrationDto>(registration);

        return dto;
    }

}
