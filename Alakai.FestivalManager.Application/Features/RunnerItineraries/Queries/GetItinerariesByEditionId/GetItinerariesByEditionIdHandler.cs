namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Queries.GetItinerariesByEditionId;

public class GetItinerariesByEditionIdHandler
{
    private readonly IRunnerItineraryRepository _itineraryRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public GetItinerariesByEditionIdHandler(IRunnerItineraryRepository itineraryRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _itineraryRepository = itineraryRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<RunnerItineraryDto>> HandleAsync(GetItinerariesByEditionIdQuery query, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(query.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{query.EditionId}' was not found.");
        }

        IReadOnlyList<RunnerItinerary> itineraries = await _itineraryRepository.GetByEditionIdAsync(query.EditionId, cancellationToken);

        return _mapper.Map<IReadOnlyList<RunnerItineraryDto>>(itineraries);
    }
}