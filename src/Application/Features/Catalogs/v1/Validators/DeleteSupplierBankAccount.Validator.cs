using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{
    public class DeleteSupplierBankAccountValidator : AbstractValidator<DeleteSupplierBankAccountCommand>
    {
        public DeleteSupplierBankAccountValidator()
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
        }
    }
}
