using Alakai.FestivalManager.Application.Contracts.Repositories;

namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.UpdateCompetition;

public class UpdateCompetitionHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ICompetitionCapacityRepository _competitionCapacityRepository;
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public UpdateCompetitionHandler(ICompetitionRepository competitionRepository, IEditionRepository editionRepository, IMapper mapper, 
        ICompetitionCapacityRepository competitionCapacityRepository, ICompetitionEntryRepository competitionEntryRepository)
    {
        _competitionRepository = competitionRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
        _competitionCapacityRepository = competitionCapacityRepository;
        _competitionEntryRepository = competitionEntryRepository;
    }

    public async Task<CompetitionDto> HandleAsync(UpdateCompetitionCommand command, CancellationToken cancellationToken = default)
    {
        Competition? competition = await _competitionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (competition is null)
            throw new NotFoundException($"Competition with id '{command.Id}' was not found.");

        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");

        _mapper.Map(command, competition);
        competition.SetUpdated();

        IReadOnlyList<CompetitionCapacity> existingCapacities =
            await _competitionCapacityRepository.GetByCompetitionIdAsync(competition.Id, cancellationToken);

        foreach (var existing in existingCapacities)
        {
            var incoming = command.Capacities
                .FirstOrDefault(c =>
                    c.MixAndMatchLevel == existing.MixAndMatchLevel &&
                    c.DanceRole == existing.DanceRole);

            if (incoming != null)
            {
                existing.Capacity = incoming.Capacity;
                existing.SortOrder = incoming.SortOrder;
                existing.IsActive = true;
            }
            else
            {
                bool hasEntries = await _competitionEntryRepository.ExistsByCapacityIdAsync(existing.Id, cancellationToken);
                if (hasEntries)
                {
                    existing.IsActive = false;
                }
                else
                {
                    _competitionCapacityRepository.Delete(existing);
                }
            }
        }

        foreach (var incoming in command.Capacities)
        {
            bool exists = existingCapacities.Any(e =>
                e.MixAndMatchLevel == incoming.MixAndMatchLevel &&
                e.DanceRole == incoming.DanceRole);

            if (!exists)
            {
                CompetitionCapacity newCap = new()
                {
                    CompetitionId = competition.Id,
                    MixAndMatchLevel = incoming.MixAndMatchLevel,
                    DanceRole = incoming.DanceRole,
                    Capacity = incoming.Capacity,
                    SortOrder = incoming.SortOrder,
                    IsActive = true
                };

                await _competitionCapacityRepository.AddAsync(newCap, cancellationToken);
            }
        }

        await _competitionRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CompetitionDto>(competition);
    }

}
