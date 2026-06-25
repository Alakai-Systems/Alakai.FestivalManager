namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Validators;

public class UpdateDiscountCodeCommandValidator : AbstractValidator<UpdateDiscountCodeCommand>
{
    public UpdateDiscountCodeCommandValidator()
    {
        RuleFor(d => d.Id).NotEmpty();
        RuleFor(d => d.EditionId).NotEmpty();
        RuleFor(d => d.Code).NotEmpty().MaximumLength(50);
        RuleFor(d => d.Name).NotEmpty().MaximumLength(150);
        RuleFor(d => d.Description).MaximumLength(1000);
        RuleFor(d => d.DiscountType).IsInEnum();
        RuleFor(d => d.DiscountValue).GreaterThan(0);
        RuleFor(d => d.ActivationType).IsInEnum();
        RuleFor(d => d.ActivationThreshold)
            .GreaterThan(0)
            .When(d => d.ActivationType == DiscountActivationType.AfterThreshold);

        RuleFor(d => d.ActivationThreshold)
            .Must(value => value is null || value == 0)
            .When(d => d.ActivationType == DiscountActivationType.Immediate)
            .WithMessage("Activation threshold must be empty when activation type is immediate.");
        RuleFor(d => d.MaxUses).GreaterThan(0).When(d => d.MaxUses.HasValue);
        RuleFor(d => d.CurrentUses).GreaterThanOrEqualTo(0);
        RuleFor(d => d.EndsAt).GreaterThan(d => d.StartsAt).When(d => d.StartsAt.HasValue && d.EndsAt.HasValue);
    }
}
