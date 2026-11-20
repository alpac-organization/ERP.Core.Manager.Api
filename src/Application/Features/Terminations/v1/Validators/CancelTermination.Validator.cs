using FluentValidation;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Validators
{
   public class CancelTerminationValidator : BaseRequestValidator<CancelTerminationCommand>
   {
      public CancelTerminationValidator()
      {
         RuleFor(x => x.TerminationId)
            .NotEmpty().WithMessage("El id del expediente de baja es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id del expediente de baja no es válido.");

         RuleFor(x => x.CancellationReason)
            .NotEmpty().WithMessage("El motivo de anulación es requerido.")
            .MaximumLength(500).WithMessage("El motivo de anulación no puede exceder 500 caracteres.");
      }
   }
}
