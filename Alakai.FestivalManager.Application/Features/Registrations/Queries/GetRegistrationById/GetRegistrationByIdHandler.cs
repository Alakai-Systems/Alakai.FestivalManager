namespace Alakai.FestivalManager.Application.Features.Registrations.Queries.GetRegistrationById;

public class GetRegistrationByIdHandler
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IMapper _mapper;

    public GetRegistrationByIdHandler(IRegistrationRepository registrationRepository, IMapper mapper)
    {
        _registrationRepository = registrationRepository;
        _mapper = mapper;
    }

    public async Task<RegistrationDto?> HandleAsync(GetRegistrationByIdQuery query, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _registrationRepository.GetByIdAsync(query.Id, cancellationToken);

        if (registration is null)
        {
            return null;
        }

        RegistrationDto dto = _mapper.Map<RegistrationDto>(registration);

        return dto;
    }
}
