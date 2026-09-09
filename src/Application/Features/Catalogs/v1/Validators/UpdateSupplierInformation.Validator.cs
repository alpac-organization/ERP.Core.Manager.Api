using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{
    public class UpdateSupplierInformationValidator : AbstractValidator<UpdateSupplierInformationCommand>
    {
        public UpdateSupplierInformationValidator()
        {
            RuleFor(x => x.SupplierId)
                .NotEmpty()
                .WithMessage("El identificador del proveedor es obligatorio");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("El identificador del usuario es obligatorio");

            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("El identificador de la empresa es obligatorio");

            RuleFor(x => x.ModuleCode)
                .NotEmpty()
                .WithMessage("El código del módulo es obligatorio");

            RuleFor(x => x.SuppliersLegalName)
                .MaximumLength(200)
                .WithMessage("La razón social no puede exceder los 200 caracteres")
                .When(x => x.SuppliersLegalName is not null);

            RuleFor(x => x.CommercialName)
                .MaximumLength(200)
                .WithMessage("El nombre comercial no puede exceder los 200 caracteres")
                .When(x => x.CommercialName is not null);

            RuleFor(x => x.IdentificationNumber)
                .MaximumLength(30)
                .WithMessage("El número de identificación no puede exceder los 30 caracteres")
                .When(x => x.IdentificationNumber is not null);

            RuleFor(x => x.ConstitutionType)
                .IsInEnum()
                .WithMessage("El tipo de constitución no es válido")
                .When(x => x.ConstitutionType is not null);

            RuleFor(x => x.IdentificationType)
                .IsInEnum()
                .WithMessage("El tipo de identificación no es válido")
                .When(x => x.IdentificationType is not null);

            // SupplierDetails

            RuleFor(x => x.SupplierDetails.ContactName)
                .MaximumLength(150)
                .WithMessage("El nombre de contacto no puede exceder los 150 caracteres")
                .When(x => x.SupplierDetails.ContactName is not null);

            RuleFor(x => x.SupplierDetails.ContactEmail)
                .EmailAddress()
                .WithMessage("El correo de contacto no tiene un formato válido")
                .MaximumLength(150)
                .WithMessage("El correo de contacto no puede exceder los 150 caracteres")
                .When(x => x.SupplierDetails.ContactEmail is not null);

            RuleFor(x => x.SupplierDetails.EmailSupport)
                .EmailAddress()
                .WithMessage("El correo de soporte no tiene un formato válido")
                .MaximumLength(150)
                .WithMessage("El correo de soporte no puede exceder los 150 caracteres")
                .When(x => x.SupplierDetails.EmailSupport is not null);

            RuleFor(x => x.SupplierDetails.ContactPhoneNumber)
                .Matches(@"^(\+505[\s-]?)?\d{4}[\s-]?\d{4}$")
                .WithMessage("El número de teléfono debe tener 8 dígitos, opcionalmente con +505")
                .When(x => x.SupplierDetails.ContactPhoneNumber is not null);

            RuleFor(x => x.SupplierDetails.Address)
                .MaximumLength(250)
                .WithMessage("La dirección no puede exceder los 250 caracteres")
                .When(x => x.SupplierDetails.Address is not null);

            RuleFor(x => x.SupplierDetails.ExclusiveBrandsOrParts)
                .MaximumLength(500)
                .WithMessage("Las marcas o líneas exclusivas no pueden exceder los 500 caracteres")
                .When(x => x.SupplierDetails.ExclusiveBrandsOrParts is not null);

            RuleFor(x => x.SupplierDetails.CreditLimit)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El límite de crédito no puede ser negativo")
                .When(x => x.SupplierDetails.CreditLimit.HasValue);

            RuleFor(x => x.SupplierDetails.CreditCurrency)
                .IsInEnum()
                .WithMessage("La moneda de crédito no es válida")
                .When(x => x.SupplierDetails.CreditCurrency.HasValue);

            RuleFor(x => x.SupplierDetails.AlertDaysBeforeDue)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Los días de alerta no pueden ser negativos")
                .When(x => x.SupplierDetails.AlertDaysBeforeDue.HasValue);

            RuleFor(x => x.SupplierDetails.PreferredPaymentMethod)
                .IsInEnum()
                .WithMessage("El método de pago no es válido")
                .When(x => x.SupplierDetails.PreferredPaymentMethod.HasValue);
        }
    }
}