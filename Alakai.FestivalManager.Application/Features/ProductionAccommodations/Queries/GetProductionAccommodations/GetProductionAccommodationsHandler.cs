namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Queries.GetProductionAccommodations;

public class GetProductionAccommodationsHandler
{
    private readonly IProductionAccommodationRepository _accommodationRepository;
    private readonly IMapper _mapper;

    public GetProductionAccommodationsHandler(IProductionAccommodationRepository accommodationRepository, IMapper mapper)
    {
        _accommodationRepository = accommodationRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductionAccommodationDto>> HandleAsync(GetProductionAccommodationsQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ProductionAccommodation> accommodations = await _accommodationRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<ProductionAccommodationDto>>(accommodations);
    }
}