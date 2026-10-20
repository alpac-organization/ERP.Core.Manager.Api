using FluentValidation;
using ERP.Core.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class RegisterReasonsInventoryRemovalValidator : BaseRequestValidator<RegisterReasonsInventoryRemovalCommand>
{
    public RegisterReasonsInventoryRemovalValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código del concepto de salida de inventario es obligatorio.")
            .MaximumLength(50).WithMessage("El código del concepto de salida de inventario no puede superar los 50 caracteres.");
    }
}
