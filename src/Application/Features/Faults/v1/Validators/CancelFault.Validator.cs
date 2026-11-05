using FluentValidation;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Validators
{
   public class CancelFaultValidator : BaseRequestValidator<CancelFaultCommand>
   {
      public CancelFaultValidator()
      {
         RuleFor(x => x.FaultId)
            .NotEmpty().WithMessage("El id de la falta es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de la falta no es válido.");

         RuleFor(x => x.CancellationReason)
            .NotEmpty().WithMessage("El motivo de anulación es requerido.")
            .MaximumLength(500).WithMessage("El motivo de anulación no puede exceder 500 caracteres.");
      }
   }
}
