namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Queries.GetDiscountCodes;

public class GetDiscountCodesHandler
{
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IMapper _mapper;

    public GetDiscountCodesHandler(IDiscountCodeRepository discountCodeRepository, IMapper mapper)
    {
        _discountCodeRepository = discountCodeRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<DiscountCodeDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DiscountCode> discountCodes = await _discountCodeRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<DiscountCodeDto>>(discountCodes);
    }
}
