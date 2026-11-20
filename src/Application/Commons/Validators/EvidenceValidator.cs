using FluentValidation;
using ERP.Core.Manager.Api.Application.Commons.Helpers;

namespace ERP.Core.Manager.Api.Application.Commons.Validators
{
   public class EvidenceValidator : AbstractValidator<EvidenceInput>
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
            .WithMessage("El contenido en base64 es requerido.");

         RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("El tipo de contenido es requerido.")
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

      private static bool HaveMatchingExtension(EvidenceInput evidence)
      {
         if (!ContentTypeExtensions.TryGetValue(evidence.ContentType!, out var extensions))
            return false;

         var extension = Path.GetExtension(evidence.FileName);
         return !string.IsNullOrWhiteSpace(extension)
            && extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
      }
   }
}
