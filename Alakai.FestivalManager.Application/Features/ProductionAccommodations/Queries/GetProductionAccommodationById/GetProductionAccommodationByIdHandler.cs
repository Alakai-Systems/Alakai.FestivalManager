namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Queries.GetProductionAccommodationById;

public class GetProductionAccommodationByIdHandler
{
    private readonly IProductionAccommodationRepository _accommodationRepository;
    private readonly IMapper _mapper;

    public GetProductionAccommodationByIdHandler(IProductionAccommodationRepository accommodationRepository, IMapper mapper)
    {
        _accommodationRepository = accommodationRepository;
        _mapper = mapper;
    }

    public async Task<ProductionAccommodationDto?> HandleAsync(GetProductionAccommodationByIdQuery query, CancellationToken cancellationToken = default)
    {
        ProductionAccommodation? accommodation = await _accommodationRepository.GetByIdAsync(query.Id, cancellationToken);

        if (accommodation is null)
        {
            return null;
        }

        return _mapper.Map<ProductionAccommodationDto>(accommodation);
    }
}