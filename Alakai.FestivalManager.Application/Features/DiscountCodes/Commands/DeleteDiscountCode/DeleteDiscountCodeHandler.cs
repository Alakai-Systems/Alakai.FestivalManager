namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.DeleteDiscountCode;

public class DeleteDiscountCodeHandler
{
    private readonly IDiscountCodeRepository _discountCodeRepository;

    public DeleteDiscountCodeHandler(IDiscountCodeRepository discountCodeRepository)
    {
        _discountCodeRepository = discountCodeRepository;
    }

    public async Task HandleAsync(DeleteDiscountCodeCommand command, CancellationToken cancellationToken = default)
    {
        DiscountCode? discountCode = await _discountCodeRepository.GetByIdAsync(command.Id, cancellationToken);

        if (discountCode is null)
        {
            throw new NotFoundException($"Discount code with id '{command.Id}' was not found.");
        }

        _discountCodeRepository.Delete(discountCode);
        await _discountCodeRepository.SaveChangesAsync(cancellationToken);
    }
}
