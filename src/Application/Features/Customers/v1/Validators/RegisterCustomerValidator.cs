using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Validators
{
    public class RegisterCustomerValidator : AbstractValidator<RegisterCustomerCommand>
    {
        public RegisterCustomerValidator()
        {
            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty).WithMessage("No se pudo identificar al usuario autenticado.");

            RuleFor(x => x.CompanyId)
                .NotEqual(Guid.Empty).WithMessage("El identificador de la compañía es obligatorio.");

            RuleFor(x => x.ModuleCode)
                .NotEmpty().WithMessage("El código del módulo es obligatorio.")
                .MaximumLength(50).WithMessage("El código del módulo no puede superar los 50 caracteres.");

            RuleFor(x => x.Cif)
                .NotEmpty().WithMessage("El CIF es obligatorio.")
                .MaximumLength(50).WithMessage("El CIF no puede superar los 50 caracteres.");

            RuleFor(x => x.LegalName)
                .NotEmpty().WithMessage("El nombre legal es obligatorio.")
                .MaximumLength(150).WithMessage("El nombre legal no puede superar los 150 caracteres.");

            RuleFor(x => x.IdentificationNumber)
                .NotEmpty().WithMessage("El número de identificación es obligatorio.")
                .MaximumLength(50).WithMessage("El número de identificación no puede superar los 50 caracteres.");

            RuleFor(x => x.IdentificationType)
                .IsInEnum().WithMessage("El tipo de identificación no es válido.");

            RuleFor(x => x.CustomerTypeId)
                .NotEqual(Guid.Empty).WithMessage("El tipo de cliente es obligatorio.");
        }
    }
}