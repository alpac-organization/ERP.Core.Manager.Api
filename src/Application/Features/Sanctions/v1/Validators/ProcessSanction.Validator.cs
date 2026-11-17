using FluentValidation;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Validators
{
   public class ProcessSanctionValidator : BaseRequestValidator<ProcessSanctionCommand>
   {
      public ProcessSanctionValidator()
      {
         RuleFor(x => x.SanctionId)
            .NotEmpty().WithMessage("El id de la sanción es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de la sanción no es válido.");

         RuleFor(x => x.CancellationReason)
            .NotEmpty()
            .WithMessage("El motivo de cancelación es requerido.")
            .MaximumLength(500)
            .WithMessage("El motivo de cancelación no puede exceder 500 caracteres.")
            .When(x => !x.IsApproved);
      }
   }
}
