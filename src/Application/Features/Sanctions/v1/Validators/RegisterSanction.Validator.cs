using FluentValidation;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Helpers;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Validators
{
   public class RegisterSanctionValidator : BaseRequestValidator<RegisterSanctionCommand>
   {
      private static readonly SanctionType[] SanctionTypesRequiringEvidence =
      [
         SanctionType.Memorandum,
         SanctionType.Suspension,
         SanctionType.CommitmentLetter,
         SanctionType.Termination
      ];

      public RegisterSanctionValidator()
      {
         RuleFor(x => x.FaultId)
            .NotEmpty().WithMessage("El id de la falta es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id de la falta no es válido.");

         RuleFor(x => x.SanctionType)
            .IsInEnum()
            .WithMessage("El tipo de sanción no es válido.");

         RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("El motivo de la sanción es requerido.")
            .MaximumLength(500)
            .WithMessage("El motivo de la sanción no puede exceder 500 caracteres.");

         RuleFor(x => x.Evidences)
            .NotEmpty()
            .WithMessage("Debe adjuntar al menos una evidencia para este tipo de sanción.")
            .When(x => SanctionTypesRequiringEvidence.Contains(x.SanctionType));

         RuleForEach(x => x.Evidences)
            .SetValidator(new EvidenceValidator());

         RuleFor(x => x.SuspensionStartDate)
            .NotNull()
            .WithMessage("La fecha de inicio de suspensión es requerida.")
            .When(x => x.SanctionType == SanctionType.Suspension);

         RuleFor(x => x.SuspensionEndDate)
            .NotNull()
            .WithMessage("La fecha de fin de suspensión es requerida.")
            .When(x => x.SanctionType == SanctionType.Suspension);         
      }      
   }
}
