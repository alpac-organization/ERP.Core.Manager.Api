using FluentValidation;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Validators
{
   public class CloseTerminationValidator : BaseRequestValidator<CloseTerminationCommand>
   {
      public CloseTerminationValidator()
      {
         RuleFor(x => x.TerminationId)
            .NotEmpty().WithMessage("El id del expediente de baja es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id del expediente de baja no es válido.");
      }
   }
}
