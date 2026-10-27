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

         // Evidencias opcionales: si vienen ítems, cada uno se valida completo
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

      private static readonly Dictionary<string, string[]> ContentTypeExtensions = new(StringComparer.OrdinalIgnoreCase)
      {
         ["image/jpeg"] = [".jpg", ".jpeg"],
         ["image/jpg"] = [".jpg", ".jpeg"],
         ["image/png"] = [".png"],
         ["image/webp"] = [".webp"]
      };

      public EvidenceValidator()
      {
         RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("El nombre del archivo es requerido.")
            .Must(BeSafeFileName)
            .WithMessage("El nombre del archivo no es válido.")
            .MaximumLength(255)
            .WithMessage("El nombre del archivo no puede exceder 255 caracteres.");

         RuleFor(x => x.ImageBase64)
            .NotEmpty()
            .WithMessage("La imagen en base64 es requerida.");

         RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("El tipo de contenido de la imagen es requerido.")
            .Must(contentType => AllowedContentTypes.Contains(contentType!, StringComparer.OrdinalIgnoreCase))
            .WithMessage("El tipo de contenido debe ser una imagen válida (jpeg, jpg, png o webp).");

         RuleFor(x => x)
            .Must(HaveMatchingExtension)
            .WithMessage("La extensión del archivo no coincide con el tipo de contenido.")
            .When(x => !string.IsNullOrWhiteSpace(x.FileName) && !string.IsNullOrWhiteSpace(x.ContentType));
      }

      private static bool BeSafeFileName(string? fileName)
      {
         if (string.IsNullOrWhiteSpace(fileName))
            return false;

         var sanitized = Path.GetFileName(fileName);
         return !string.IsNullOrWhiteSpace(sanitized)
            && sanitized == fileName.Trim()
            && sanitized.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
      }

      private static bool HaveMatchingExtension(Evidence evidence)
      {
         if (!ContentTypeExtensions.TryGetValue(evidence.ContentType!, out var extensions))
            return false;

         var extension = Path.GetExtension(evidence.FileName);
         return !string.IsNullOrWhiteSpace(extension)
            && extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
      }
   }
}
