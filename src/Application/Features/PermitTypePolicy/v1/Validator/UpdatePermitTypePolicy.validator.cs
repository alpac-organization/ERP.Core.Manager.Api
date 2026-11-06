using FluentValidation;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.commands;

namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.validator
{
   public class UpdatePermitTypePolicyValidator : BaseRequestValidator<UpdatePermitTypePolicyCommand>
   {
      public UpdatePermitTypePolicyValidator()
      {
         RuleFor(x => x.PermitTypePolicyId)
            .NotEmpty().WithMessage("El id de la politica es requerido.");

         When(x => x.PermitApplicationType.HasValue, () =>
         {
            RuleFor(x => x.PermitApplicationType)
               .IsInEnum().WithMessage("El tipo de permiso no es válido.")
               .Must(type => type.HasValue && PermitTypePolicyRules.IsAllowed(type.Value))
               .WithMessage("La politica solo aplica a cita judicial, matrimonio, nacimiento y fallecimiento.");
         });

         When(x => x.PermitBeneficiaryScope.HasValue, () =>
         {
            RuleFor(x => x.PermitBeneficiaryScope)
               .IsInEnum().WithMessage("El alcance no es válido.")
               .NotEqual(PermitBeneficiaryScope.None)
               .WithMessage("Debe indicar si el permiso es propio o de núcleo familiar.");
         });

         When(x => x.FamilyRelationship.HasValue, () =>
         {
            RuleFor(x => x.FamilyRelationship)
               .IsInEnum().WithMessage("El parentesco no es válido.");
         });

         When(x => x.MinEvidenceCount.HasValue, () =>
         {
            RuleFor(x => x.MinEvidenceCount)
               .GreaterThanOrEqualTo(0).WithMessage("El mínimo de evidencias no puede ser negativo.");
         });

         When(x => x.MaxEvidenceCount.HasValue, () =>
         {
            RuleFor(x => x.MaxEvidenceCount)
               .GreaterThanOrEqualTo(0).WithMessage("El máximo de evidencias no puede ser negativo.");

            RuleFor(x => x.MaxEvidenceCount)
               .GreaterThanOrEqualTo(x => x.MinEvidenceCount)
               .When(x => x.MinEvidenceCount.HasValue)
               .WithMessage("El máximo de evidencias no puede ser menor al mínimo.");
         });

         When(x => x.RequiredApprovalSteps.HasValue, () =>
         {
            RuleFor(x => x.RequiredApprovalSteps)
               .InclusiveBetween(1, 2)
               .WithMessage("Los pasos de aprobación deben ser 1 o 2.");
         });

         When(x => x.MaxAmountDays.HasValue, () =>
         {
            RuleFor(x => x.MaxAmountDays)
               .GreaterThan(0)
               .WithMessage("La cantidad máxima de días debe ser mayor a cero.");
         });
      }
   }
}
