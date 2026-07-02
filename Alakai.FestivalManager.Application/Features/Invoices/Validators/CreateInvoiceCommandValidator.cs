using Alakai.FestivalManager.Application.Features.Invoices.Commands.CreateInvoice;
using FluentValidation;

namespace Alakai.FestivalManager.Application.Features.Invoices.Validators;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(c => c.RegistrationId).NotEmpty();
        RuleFor(c => c.FiscalName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.TaxId).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Address).NotEmpty().MaximumLength(300);
        RuleFor(c => c.City).NotEmpty().MaximumLength(100);
        RuleFor(c => c.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(c => c.Country).NotEmpty().MaximumLength(100);
    }
}