namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Queries.GetDiscountCodeById;

public class GetDiscountCodeByIdHandler
{
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IMapper _mapper;

    public GetDiscountCodeByIdHandler(IDiscountCodeRepository discountCodeRepository, IMapper mapper)
    {
        _discountCodeRepository = discountCodeRepository;
        _mapper = mapper;
    }

    public async Task<DiscountCodeDto> HandleAsync(GetDiscountCodeByIdQuery query, CancellationToken cancellationToken = default)
    {
        DiscountCode? discountCode = await _discountCodeRepository.GetByIdAsync(query.Id, cancellationToken);

        if (discountCode is null)
        {
            throw new NotFoundException($"Discount code with id '{query.Id}' was not found.");
        }

        return _mapper.Map<DiscountCodeDto>(discountCode);
    }
}
