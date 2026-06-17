namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.CreateDiscountCode;

public class CreateDiscountCodeHandler
{
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public CreateDiscountCodeHandler(IDiscountCodeRepository discountCodeRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _discountCodeRepository = discountCodeRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<DiscountCodeDto> HandleAsync(CreateDiscountCodeCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        string normalizedCode = command.Code.Trim().ToUpperInvariant();

        bool exists = await _discountCodeRepository.ExistsByEditionAndCodeAsync(command.EditionId, normalizedCode, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"Discount code '{normalizedCode}' already exists for this edition.");
        }

        command.Code = normalizedCode;

        DiscountCode discountCode = _mapper.Map<DiscountCode>(command);
        discountCode.CurrentUses = 0;
        discountCode.IsActive = true;

        await _discountCodeRepository.AddAsync(discountCode, cancellationToken);
        await _discountCodeRepository.SaveChangesAsync(cancellationToken);

        DiscountCodeDto dto = _mapper.Map<DiscountCodeDto>(discountCode);

        return dto;
    }
}
