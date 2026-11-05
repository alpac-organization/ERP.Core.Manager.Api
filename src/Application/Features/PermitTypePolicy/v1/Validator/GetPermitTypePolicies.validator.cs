using FluentValidation;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.validator
{
   public class GetPermitTypePoliciesValidator : AbstractValidator<GetPermitTypePoliciesQuery>
   {
      public GetPermitTypePoliciesValidator()
      {
         RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("El número de página debe ser mayor que 0.");

         RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("El tamaño de página debe ser mayor que 0.");

         RuleFor(x => x.PermitApplicationType)
            .IsInEnum()
            .When(x => x.PermitApplicationType.HasValue)
            .WithMessage("El tipo de permiso no es válido.");
      }
   }
}
