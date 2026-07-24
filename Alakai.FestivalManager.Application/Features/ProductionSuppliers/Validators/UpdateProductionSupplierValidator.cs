namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Validators;

public class UpdateProductionSupplierValidator : AbstractValidator<UpdateProductionSupplierCommand>
{
    public UpdateProductionSupplierValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.EditionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ServiceType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ContactName).MaximumLength(150);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}