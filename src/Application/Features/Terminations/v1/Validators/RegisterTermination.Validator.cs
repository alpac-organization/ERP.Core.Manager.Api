using FluentValidation;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Validators
{
   public class RegisterTerminationValidator : BaseRequestValidator<RegisterTerminationCommand>
   {
      public RegisterTerminationValidator()
      {
         RuleFor(x => x.CollaboratorId)
            .NotEmpty().WithMessage("El id del colaborador es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id del colaborador no es válido.");

         RuleFor(x => x.TerminationType)
            .IsInEnum()
            .WithMessage("El tipo de terminación no es válido.");

         RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("El motivo de la baja es requerido.")
            .MaximumLength(500)
            .WithMessage("El motivo de la baja no puede exceder 500 caracteres.");

         RuleFor(x => x.DepartureDate)
            .Must(date => date != default)
            .WithMessage("La fecha de terminación es requerida.");

         RuleFor(x => x.NoticeDate)
            .LessThanOrEqualTo(x => x.DepartureDate)
            .WithMessage("La fecha de preaviso no puede ser posterior a la fecha de terminación.")
            .When(x => x.NoticeDate.HasValue);

         RuleFor(x => x.NoticeDays)
            .GreaterThan(0)
            .WithMessage("Los días de preaviso deben ser mayor a cero.")
            .When(x => x.NoticeDays.HasValue);

         RuleFor(x => x.SanctionId)
            .NotEqual(Guid.Empty)
            .WithMessage("El id de la sanción no es válido.")
            .When(x => x.SanctionId.HasValue);

         RuleFor(x => x.SanctionId)
            .NotNull()
            .WithMessage("El id de la sanción es requerido para un despido justificado.")
            .NotEqual(Guid.Empty)
            .WithMessage("El id de la sanción no es válido.")
            .When(x => x.TerminationType == TerminationType.JustifiedDismissal);

         RuleForEach(x => x.Evidences)
            .SetValidator(new EvidenceValidator())
            .When(x => x.Evidences != null && x.Evidences.Count > 0);
      }
   }
}
