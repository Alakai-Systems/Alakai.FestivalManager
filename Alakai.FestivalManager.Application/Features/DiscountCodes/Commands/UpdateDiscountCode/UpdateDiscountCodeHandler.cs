namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.UpdateDiscountCode;

public class UpdateDiscountCodeHandler
{
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public UpdateDiscountCodeHandler(IDiscountCodeRepository discountCodeRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _discountCodeRepository = discountCodeRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<DiscountCodeDto> HandleAsync(UpdateDiscountCodeCommand command, CancellationToken cancellationToken = default)
    {
        DiscountCode? discountCode = await _discountCodeRepository.GetByIdAsync(command.Id, cancellationToken);

        if (discountCode is null)
        {
            throw new NotFoundException($"Discount code with id '{command.Id}' was not found.");
        }

        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        string normalizedCode = command.Code.Trim().ToUpperInvariant();

        DiscountCode? existing = await _discountCodeRepository.GetByEditionAndCodeAsync(command.EditionId, normalizedCode, cancellationToken);

        if (existing is not null && existing.Id != command.Id)
        {
            throw new BusinessRuleException($"Discount code '{normalizedCode}' already exists for this edition.");
        }

        command.Code = normalizedCode;

        _mapper.Map(command, discountCode);
        discountCode.SetUpdated();

        await _discountCodeRepository.SaveChangesAsync(cancellationToken);

        DiscountCodeDto dto = _mapper.Map<DiscountCodeDto>(discountCode);

        return dto;
    }
}
