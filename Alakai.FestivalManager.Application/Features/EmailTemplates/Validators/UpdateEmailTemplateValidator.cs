namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Validators;

public class UpdateEmailTemplateValidator : AbstractValidator<UpdateEmailTemplateCommand>
{
    public UpdateEmailTemplateValidator()
    {
        RuleFor(e => e.Id)
            .NotEmpty();

        RuleFor(e => e.TemplateKey)
            .IsInEnum();

        RuleFor(e => e.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(e => e.Subject)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(e => e.BodyHtml)
            .NotEmpty();
    }
}
