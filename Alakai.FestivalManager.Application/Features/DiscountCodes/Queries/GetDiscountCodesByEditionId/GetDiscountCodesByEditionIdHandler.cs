namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Queries.GetDiscountCodesByEditionId;

public class GetDiscountCodesByEditionIdHandler
{
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IMapper _mapper;

    public GetDiscountCodesByEditionIdHandler(IDiscountCodeRepository discountCodeRepository, IMapper mapper)
    {
        _discountCodeRepository = discountCodeRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<DiscountCodeDto>> HandleAsync(GetDiscountCodesByEditionIdQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DiscountCode> discountCodes = await _discountCodeRepository.GetByEditionIdAsync(query.EditionId, cancellationToken);
        return _mapper.Map<IReadOnlyList<DiscountCodeDto>>(discountCodes);
    }
}
