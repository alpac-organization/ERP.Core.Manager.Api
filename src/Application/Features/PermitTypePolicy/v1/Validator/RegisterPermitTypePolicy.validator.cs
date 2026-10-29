using FluentValidation;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Validators;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.commands;

namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.validator
{
   public class RegisterPermitTypePolicyValidator : BaseRequestValidator<RegisterPermissionTypePolicyCommand>
   {
      public RegisterPermitTypePolicyValidator()
      {
         RuleFor(x => x.PermitApplicationType)
            .IsInEnum().WithMessage("El tipo de permiso no es válido.");

         RuleFor(x => x.PermitBeneficiaryScope)
            .IsInEnum().WithMessage("El alcance no es válido.")
            .NotEqual(PermitBeneficiaryScope.None)
            .WithMessage("Debe indicar si el permiso es propio o de núcleo familiar.");

         RuleFor(x => x.FamilyRelationship)
            .Equal(FamilyRelationship.None)
            .When(x => x.PermitBeneficiaryScope == PermitBeneficiaryScope.Own)
            .WithMessage("Un permiso propio no lleva parentesco.");

         RuleFor(x => x.FamilyRelationship)
            .NotEqual(FamilyRelationship.None)
            .When(x => x.PermitBeneficiaryScope == PermitBeneficiaryScope.FamilyNucleus)
            .WithMessage("Debe indicar el parentesco.");

         RuleFor(x => x.MinEvidenceCount)
            .GreaterThanOrEqualTo(0).WithMessage("El mínimo de evidencias no puede ser negativo.");

         RuleFor(x => x.MinEvidenceCount)
            .GreaterThan(0)
            .When(x => x.RequiresEvidence)
            .WithMessage("Si exige evidencia, el mínimo debe ser al menos 1.");

         RuleFor(x => x.MaxEvidenceCount)
            .GreaterThanOrEqualTo(x => x.MinEvidenceCount)
            .WithMessage("El máximo de evidencias no puede ser menor al mínimo.");

         RuleFor(x => x.RequiredApprovalSteps)
            .InclusiveBetween(1, 2)
            .WithMessage("Los pasos de aprobación deben ser 1 o 2.");

         RuleFor(x => x.MaxAmountDays)
            .GreaterThan(0)
            .When(x => x.MaxAmountDays.HasValue)
            .WithMessage("La cantidad máxima de días debe ser mayor a cero.");
      }
   }
}