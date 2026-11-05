using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.commands;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Handlers
{
   public class UpdatePermitTypePolicyHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager)
      : BaseValidatorHandler<UpdatePermitTypePolicyCommand, bool>(_unitOfWork, _errorManager)
   {
      public override async Task<bool> Handle(UpdatePermitTypePolicyCommand request, CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

         if (!access.IsSuccess)
         {
            return access.ErrorResponse!;
         }

         if (access.Role?.RoleType != RoleType.Administrator)
         {
            return _errorManager.ThrowBadRequest<bool>("No tiene permiso para actualizar un tipo de politica para permisos", "ERP:01");
         }

         var policy = await _unitOfWork.PermitTypePolicyRepository.Entities
            .FirstOrDefaultAsync(item => item.Id == request.PermitTypePolicyId && item.CompanyId == request.CompanyId, cancellationToken);

         if (policy is null)
         {
            return _errorManager.ThrowBadRequest<bool>("No existe la politica de permiso", "ERP:02");
         }

         var type = request.PermitApplicationType ?? policy.PermitApplicationType;
         if (!PermitTypePolicyRules.IsAllowed(type))
         {
            return _errorManager.ThrowBadRequest<bool>("La politica solo aplica a cita judicial, matrimonio, nacimiento y fallecimiento.", "ERP:TYPE");
         }

         var scope = request.PermitBeneficiaryScope ?? policy.BeneficiaryScope;
         var relationship = request.FamilyRelationship ?? policy.FamilyRelationship;

         if (scope == PermitBeneficiaryScope.None)
         {
            return _errorManager.ThrowBadRequest<bool>("Debe indicar si el permiso es propio o de núcleo familiar.", "ERP:03");
         }

         if (scope == PermitBeneficiaryScope.Own && relationship != FamilyRelationship.None)
         {
            return _errorManager.ThrowBadRequest<bool>("Un permiso propio no lleva parentesco.", "ERP:04");
         }

         if (scope == PermitBeneficiaryScope.FamilyNucleus && relationship == FamilyRelationship.None)
         {
            return _errorManager.ThrowBadRequest<bool>("Debe indicar el parentesco.", "ERP:05");
         }

         var duplicated = await _unitOfWork.PermitTypePolicyRepository.Entities.AnyAsync(item =>
            item.Id != policy.Id &&
            item.CompanyId == request.CompanyId &&
            item.PermitApplicationType == type &&
            item.BeneficiaryScope == scope &&
            item.FamilyRelationship == relationship, cancellationToken);

         if (duplicated)
         {
            return _errorManager.ThrowBadRequest<bool>("Ya existe una politica para este tipo de permiso", "ERP:06");
         }

         var requiresEvidence = request.RequiresEvidence ?? policy.RequiresEvidence;
         var minEvidence = request.MinEvidenceCount ?? policy.MinEvidenceCount;
         var maxEvidence = request.MaxEvidenceCount ?? policy.MaxEvidenceCount;

         if (!requiresEvidence)
         {
            minEvidence = 0;
            maxEvidence = 0;
         }

         if (minEvidence < 0)
         {
            return _errorManager.ThrowBadRequest<bool>("El mínimo de evidencias no puede ser negativo.", "ERP:07");
         }

         if (requiresEvidence && minEvidence < 1)
         {
            return _errorManager.ThrowBadRequest<bool>("Si exige evidencia, el mínimo debe ser al menos 1.", "ERP:08");
         }

         if (maxEvidence < minEvidence)
         {
            return _errorManager.ThrowBadRequest<bool>("El máximo de evidencias no puede ser menor al mínimo.", "ERP:09");
         }

         policy.PermitApplicationType = type;
         policy.BeneficiaryScope = scope;
         policy.FamilyRelationship = relationship;
         policy.IsActive = request.IsActive ?? policy.IsActive;
         policy.RequiresEvidence = requiresEvidence;
         policy.ConsumesVacationBalance = request.ConsumesVacationBalance ?? policy.ConsumesVacationBalance;
         policy.MinEvidenceCount = minEvidence;
         policy.MaxEvidenceCount = maxEvidence;
         policy.RequiredApprovalSteps = request.RequiredApprovalSteps ?? policy.RequiredApprovalSteps;
         policy.MaxAmountDays = request.MaxAmountDays ?? policy.MaxAmountDays;

         await _unitOfWork.PermitTypePolicyRepository.UpdateAsync(policy);
         await _unitOfWork.SaveChangesAsync(cancellationToken);

         return true;
      }
   }
}
