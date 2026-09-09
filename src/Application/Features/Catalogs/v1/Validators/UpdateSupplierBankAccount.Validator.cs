using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{
    public class UpdateSupplierBankAccountValidator : AbstractValidator<UpdateSupplierBankAccountCommand>
    {
        public UpdateSupplierBankAccountValidator()
        {
            RuleFor(x => x.SupplierId)
                .NotEmpty().WithMessage("El identificador del proveedor es obligatorio.");

            RuleFor(x => x.BankAccountId)
                .NotEmpty().WithMessage("El identificador de la cuenta bancaria es obligatorio.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El id de usuario es obligatorio.");

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("El id de la empresa es obligatorio.");

            RuleFor(x => x.ModuleCode)
                .NotEmpty().WithMessage("El código del módulo es obligatorio.");

            RuleFor(x => x.BankName)
                .MaximumLength(100).WithMessage("El nombre del banco no puede exceder 100 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.BankName));

            RuleFor(x => x.AccountNumber)
                .MaximumLength(50).WithMessage("El número de cuenta no puede exceder 50 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.AccountNumber));

            RuleFor(x => x.AccountType)
                .IsInEnum().WithMessage("El tipo de cuenta bancaria es inválido.")
                .When(x => x.AccountType.HasValue);

            RuleFor(x => x.Currency)
                .IsInEnum().WithMessage("La moneda de la cuenta bancaria es inválida.")
                .When(x => x.Currency.HasValue);

            RuleFor(x => x.AccountHolderName)
                .MaximumLength(200).WithMessage("El nombre del titular no puede exceder 200 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.AccountHolderName));

            RuleFor(x => x.AccountHolderIdentification)
                .MaximumLength(50).WithMessage("La identificación del titular no puede exceder 50 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.AccountHolderIdentification));
        }
    }
}
