using FluentValidation;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Validators
{
   public class RegisterFaultValidator : BaseRequestValidator<RegisterFaultCommand>
   {
      public RegisterFaultValidator()
      {
         RuleFor(x => x.Faults)
            .NotEmpty()
            .WithMessage("Debe registrar al menos una falta disciplinaria.");

         RuleForEach(x => x.Faults)
            .SetValidator(new FaultValidator());
      }
   }

   public class FaultValidator : AbstractValidator<Fault>
   {
      public FaultValidator()
      {
         RuleFor(x => x.CollaboratorId)
            .NotEmpty().WithMessage("El id del colaborador es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id del colaborador no es válido.");

         RuleFor(x => x.FaultCatalogId)
            .NotEmpty().WithMessage("El id del catálogo de falta es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El id del catálogo de falta no es válido.");

         RuleFor(x => x.OccurrenceDate)
            .NotEmpty().WithMessage("La fecha de ocurrencia es requerida.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha de ocurrencia no puede ser futura.");

         RuleFor(x => x.Source)
            .IsInEnum().WithMessage("El origen de la falta no es válido.");

         RuleFor(x => x.ParentFaultId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("El id de la falta padre no es válido.");

         RuleForEach(x => x.Evidences)
            .SetValidator(new EvidenceValidator());
      }
   }

   public class EvidenceValidator : AbstractValidator<Evidence>
   {
      private static readonly string[] AllowedContentTypes =
      [
         "image/jpeg",
         "image/jpg",
         "image/png",
         "image/webp"
      ];

      public EvidenceValidator()
      {
         RuleFor(x => x.ImageBase64)
            .NotEmpty()
            .WithMessage("La imagen en base64 es requerida.");

         RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("El tipo de contenido de la imagen es requerido.")
            .Must(contentType => AllowedContentTypes.Contains(contentType!, StringComparer.OrdinalIgnoreCase))
            .WithMessage("El tipo de contenido debe ser una imagen válida (jpeg, jpg, png o webp).");
      }
   }
}
