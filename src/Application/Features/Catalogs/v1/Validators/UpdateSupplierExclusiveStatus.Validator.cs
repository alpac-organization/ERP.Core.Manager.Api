using FluentValidation;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class UpdateSupplierExclusiveStatusValidator : BaseRequestValidator<UpdateSupplierExclusiveStatusCommand>
{
    public UpdateSupplierExclusiveStatusValidator()
    {
        CatalogValidationRules.ApplySupplierIdRules(this, x => x.SupplierId);

        RuleFor(x => x.ExclusiveStatus)
            .Must(status => status is SupplierExclusiveStatus.Approved or SupplierExclusiveStatus.Rejected)
            .WithMessage("Solo se permite aprobar (Approved) o rechazar (Rejected) el estado exclusivo.");

        RuleFor(x => x.Comments)
            .NotEmpty()
            .WithMessage("El comentario es obligatorio al aprobar o rechazar la exclusividad.")
            .MaximumLength(500)
            .WithMessage("El comentario no puede exceder los 500 caracteres.");
    }
}
