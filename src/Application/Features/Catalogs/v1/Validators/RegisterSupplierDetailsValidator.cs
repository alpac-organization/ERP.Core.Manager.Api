using System.Text.RegularExpressions;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using FluentValidation;
namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators
{
public partial class SupplierDetailsValidator : AbstractValidator<SupplierDetails> 
{
   [GeneratedRegex(@"^(\+505[\s-]?)?\d{4}[\s-]?\d{4}$", RegexOptions.CultureInvariant)]
   private static partial Regex PhoneRegex();
    public SupplierDetailsValidator()
    {
        RuleFor(x => x.CreditDays)
            .GreaterThanOrEqualTo(0).WithMessage("Los días de crédito no pueden ser negativos.");

        RuleFor(x => x.CreditDays)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Los días de crédito deben contener al menos un día.")
            .When(x => x.HasCredit);

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.EmailSupport)
            .EmailAddress().WithMessage("El correo de soporte no es válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.EmailSupport));

        RuleFor(x => x.ContactName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactName));

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("El correo del contacto no es válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        RuleFor(x => x.ContactPhoneNumber)
            .Matches(PhoneRegex())
            .WithMessage("El número de teléfono debe tener 8 dígitos, opcionalmente con +505")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactPhoneNumber));

        RuleFor(x => x.ExclusiveStatus)
            .Must(status => status is SupplierExclusiveStatus.None or SupplierExclusiveStatus.PendingReview)
            .WithMessage("Al registrar un proveedor solo se permite ExclusiveStatus None o PendingReview.");

        RuleFor(x => x.ExclusiveBrandsOrParts)
            .MaximumLength(500).WithMessage("Las marcas o líneas exclusivas no pueden exceder 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.ExclusiveBrandsOrParts));

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("El límite de crédito no puede ser negativo.")
            .When(x => x.CreditLimit.HasValue);

        RuleFor(x => x.CreditCurrency)
            .IsInEnum().WithMessage("La moneda de crédito es inválida.")
            .When(x => x.CreditCurrency.HasValue);

        RuleFor(x => x.Currency)
            .IsInEnum().WithMessage("La moneda del proveedor es inválida.");

        RuleFor(x => x.AlertDaysBeforeDue)
            .GreaterThanOrEqualTo(0).WithMessage("Los días de alerta no pueden ser negativos.");
        }
   }
}