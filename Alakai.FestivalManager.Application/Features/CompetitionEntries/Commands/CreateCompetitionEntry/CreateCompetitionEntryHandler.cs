namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Commands.CreateCompetitionEntry;

public class CreateCompetitionEntryHandler
{
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IMapper _mapper;

    public CreateCompetitionEntryHandler(ICompetitionEntryRepository competitionEntryRepository, ICompetitionRepository competitionRepository, IRegistrationRepository registrationRepository, IMapper mapper)
    {
        _competitionEntryRepository = competitionEntryRepository;
        _competitionRepository = competitionRepository;
        _registrationRepository = registrationRepository;
        _mapper = mapper;
    }

    public async Task<CompetitionEntryDto> HandleAsync(CreateCompetitionEntryCommand command, CancellationToken cancellationToken = default)
    {
        Competition? competition = await _competitionRepository.GetByIdAsync(command.CompetitionId, cancellationToken);

        if (competition is null)
        {
            throw new NotFoundException($"Competition with id '{command.CompetitionId}' was not found.");
        }

        Registration? registration = await _registrationRepository.GetByIdAsync(command.RegistrationId, cancellationToken);

        if (registration is null)
        {
            throw new NotFoundException($"Registration with id '{command.RegistrationId}' was not found.");
        }

        if (command.PartnerRegistrationId.HasValue)
        {
            Registration? partnerRegistration = await _registrationRepository.GetByIdAsync(command.PartnerRegistrationId.Value, cancellationToken);

            if (partnerRegistration is null)
            {
                throw new NotFoundException($"Partner registration with id '{command.PartnerRegistrationId}' was not found.");
            }
        }

        bool exists = await _competitionEntryRepository.ExistsByCompetitionAndRegistrationAsync(command.CompetitionId, command.RegistrationId, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException("This registration is already entered in this competition.");
        }

        CompetitionEntry entry = _mapper.Map<CompetitionEntry>(command);
        entry.Status = command.PartnerRegistrationId.HasValue || !competition.RequiresPartner ? CompetitionEntryStatus.Registered : CompetitionEntryStatus.WaitingPartner;
        entry.IsActive = true;

        await _competitionEntryRepository.AddAsync(entry, cancellationToken);
        await _competitionEntryRepository.SaveChangesAsync(cancellationToken);

        CompetitionEntryDto dto = _mapper.Map<CompetitionEntryDto>(entry);

        return dto;
    }
}
