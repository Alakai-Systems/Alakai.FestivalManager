namespace Alakai.FestivalManager.Application.Features.Users.Validators;

public class BulkImportUsersCommandValidator : AbstractValidator<BulkImportUsersCommand>
{
    public BulkImportUsersCommandValidator()
    {
        RuleFor(command => command.Rows)
            .NotEmpty()
            .WithMessage("The CSV file has no rows to import.");

        RuleFor(command => command.Rows.Count)
            .LessThanOrEqualTo(20000)
            .WithMessage("A maximum of 20000 rows can be imported in a single file.");
    }
}