namespace Alakai.FestivalManager.Application.Features.ProductionPeople.Validators;

public class CreateProductionPersonValidator : AbstractValidator<CreateProductionPersonCommand>
{
    public CreateProductionPersonValidator()
    {
        RuleFor(x => x.EditionId)
            .NotEmpty();

        RuleFor(x => x.Category)
            .IsInEnum();

        RuleFor(x => x.RoleTitle)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.Phone)
            .MaximumLength(30);

        RuleFor(x => x.DocumentType)
            .IsInEnum();

        RuleFor(x => x.DocumentNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Nationality)
            .MaximumLength(100);
    }
}