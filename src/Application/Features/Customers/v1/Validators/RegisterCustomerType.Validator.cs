using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Validators
{
    public class RegisterCustomerTypeValidator : AbstractValidator<RegisterCustomerTypeCommand>
    {
        public RegisterCustomerTypeValidator()
        {
            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty).WithMessage("No se pudo identificar al usuario autenticado.");

            RuleFor(x => x.CompanyId)
                .NotEqual(Guid.Empty).WithMessage("El identificador de la compañía es obligatorio.");

            RuleFor(x => x.ModuleCode)
                .NotEmpty().WithMessage("El código del módulo es obligatorio.")
                .MaximumLength(50).WithMessage("El código del módulo no puede superar los 50 caracteres.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("El código es obligatorio.")
                .MaximumLength(20).WithMessage("El código no puede superar los 20 caracteres.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");
        }
    }
}