using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using FluentValidation;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{

public class BankAccountValidator : AbstractValidator<SupplierBankAccountCommand> 
{
    public BankAccountValidator()
    {
        RuleFor(b => b.BankName)
            .NotEmpty().WithMessage("El nombre del banco es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre del banco no puede exceder 100 caracteres.");

        RuleFor(b => b.AccountNumber)
            .NotEmpty().WithMessage("El número de cuenta es obligatorio.")
            .MaximumLength(50).WithMessage("El número de cuenta no puede exceder 50 caracteres.");

        RuleFor(b => b.AccountType)
            .IsInEnum().WithMessage("El tipo de cuenta bancaria es inválido.");

        RuleFor(b => b.Currency)
            .IsInEnum().WithMessage("La moneda de la cuenta bancaria es inválida.");

        RuleFor(b => b.AccountHolderName)
            .NotEmpty().WithMessage("El nombre del titular de la cuenta es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre del titular no puede exceder 200 caracteres.");

        RuleFor(b => b.AccountHolderIdentification)
            .MaximumLength(50).WithMessage("La identificación del titular no puede exceder 50 caracteres.")
            .When(b => !string.IsNullOrWhiteSpace(b.AccountHolderIdentification));
    }
}
}