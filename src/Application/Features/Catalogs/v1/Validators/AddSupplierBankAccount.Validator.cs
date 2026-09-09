using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{
    public class AddSupplierBankAccountValidator : AbstractValidator<AddSupplierBankAccountCommand>
    {
        public AddSupplierBankAccountValidator()
        {
            RuleFor(x => x.SupplierId)
                .NotEmpty().WithMessage("El identificador del proveedor es obligatorio.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El id de usuario es obligatorio.");

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("El id de la empresa es obligatorio.");

            RuleFor(x => x.ModuleCode)
                .NotEmpty().WithMessage("El código del módulo es obligatorio.");

            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("El nombre del banco es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre del banco no puede exceder 100 caracteres.");

            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("El número de cuenta es obligatorio.")
                .MaximumLength(50).WithMessage("El número de cuenta no puede exceder 50 caracteres.");

            RuleFor(x => x.AccountType)
                .IsInEnum().WithMessage("El tipo de cuenta bancaria es inválido.");

            RuleFor(x => x.Currency)
                .IsInEnum().WithMessage("La moneda de la cuenta bancaria es inválida.");

            RuleFor(x => x.AccountHolderName)
                .NotEmpty().WithMessage("El nombre del titular de la cuenta es obligatorio.")
                .MaximumLength(200).WithMessage("El nombre del titular no puede exceder 200 caracteres.");

            RuleFor(x => x.AccountHolderIdentification)
                .MaximumLength(50).WithMessage("La identificación del titular no puede exceder 50 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.AccountHolderIdentification));
        }
    }
}
