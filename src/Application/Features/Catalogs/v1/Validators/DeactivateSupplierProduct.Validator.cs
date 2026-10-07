using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class DeactivateSupplierProductValidator : BaseRequestValidator<DeactivateSupplierProductCommand>
{
    public DeactivateSupplierProductValidator()
    {
        CatalogValidationRules.ApplySupplierIdRules(this, x => x.SupplierId);
        CatalogValidationRules.ApplyProductIdRules(this, x => x.ProductId);
    }
}
