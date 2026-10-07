using FluentValidation;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Commons.Validators;

public abstract class BaseRequestValidator<T> : AbstractValidator<T>
    where T : BaseRequest
{
    protected BaseRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El id de usuario es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de usuario no es válido.");

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("El id de la empresa es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de la empresa no es válido.");

        RuleFor(x => x.ModuleCode)
            .NotEmpty().WithMessage("El código de módulo es requerido.");
    }
}
