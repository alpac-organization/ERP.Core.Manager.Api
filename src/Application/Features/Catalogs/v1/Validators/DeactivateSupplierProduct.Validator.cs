using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class DeactivateSupplierProductValidator : BaseRequestValidator<DeactivateSupplierProductCommand>
{
    public DeactivateSupplierProductValidator()
    {
        SupplierProductValidationRules.ApplySupplierIdRules(this, x => x.SupplierId);
        SupplierProductValidationRules.ApplyProductIdRules(this, x => x.ProductId);
    }
}
