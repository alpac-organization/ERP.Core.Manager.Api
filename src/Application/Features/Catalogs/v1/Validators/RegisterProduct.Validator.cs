using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using FluentValidation;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Validators;

public class RegisterProductValidator : AbstractValidator<RegisterProductCommand>
{
   public RegisterProductValidator()
   {
      RuleFor(x => x.UserId)
          .NotEmpty().WithMessage("El id de usuario es requerido.")
          .NotEqual(Guid.Empty).WithMessage("El id de usuario no es válido");

      RuleFor(x => x.CompanyId)
          .NotEmpty().WithMessage("El id de la empresa es requerido.")
          .NotEqual(Guid.Empty).WithMessage("El id de la empresa no es válido.");

      RuleFor(x => x.ModuleCode)
              .NotEmpty().WithMessage("El código de módulo es requerido.");

      RuleFor(x => x.ProductName)
          .NotEmpty().WithMessage("El nombre del producto es obligatorio.")
          .MaximumLength(30).WithMessage("El nombre del producto no puede exceder los 200 caracteres.");


      RuleFor(x => x.CategoryId)
          .NotEmpty().WithMessage("La categoría es obligatoria.")
          .NotEqual(Guid.Empty).WithMessage("El id de la categoría no es válido.");
   }
}