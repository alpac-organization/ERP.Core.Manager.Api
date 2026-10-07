using FluentValidation;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class UpdateSupplierExclusiveStatusValidator : AbstractValidator<UpdateSupplierExclusiveStatusCommand>
{
    public UpdateSupplierExclusiveStatusValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El id de usuario es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de usuario no es válido.");

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("El id de la empresa es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de la empresa no es válido.");

        RuleFor(x => x.ModuleCode)
            .NotEmpty().WithMessage("El código de módulo es requerido.");

        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("El id del proveedor es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id del proveedor no es válido.");

        RuleFor(x => x.ExclusiveStatus)
            .Must(status => status is SupplierExclusiveStatus.Approved or SupplierExclusiveStatus.Rejected)
            .WithMessage("Solo se permite aprobar (Approved) o rechazar (Rejected) el estado exclusivo.");

        RuleFor(x => x.Comments)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Comments));
    }
}
