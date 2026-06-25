namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Commands.DeleteDiscountCode;

public class DeleteDiscountCodeHandler
{
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IRegistrationRepository _registrationRepository;

    public DeleteDiscountCodeHandler(IDiscountCodeRepository discountCodeRepository, IRegistrationRepository registrationRepository)
    {
        _discountCodeRepository = discountCodeRepository;
        _registrationRepository = registrationRepository;
    }

    public async Task HandleAsync(DeleteDiscountCodeCommand command, CancellationToken cancellationToken = default)
    {
        DiscountCode? discountCode = await _discountCodeRepository.GetByIdAsync(command.Id, cancellationToken);

        int uses = await _registrationRepository.CountByDiscountCodeAsync(command.Id, cancellationToken);

        if (uses > 0)
        {
            throw new BusinessRuleException("This discount code cannot be deleted because it is assigned to existing registrations.");
        }

        if (discountCode is null)
        {
            throw new NotFoundException($"Discount code with id '{command.Id}' was not found.");
        }

        _discountCodeRepository.Delete(discountCode);
        await _discountCodeRepository.SaveChangesAsync(cancellationToken);
    }
}
