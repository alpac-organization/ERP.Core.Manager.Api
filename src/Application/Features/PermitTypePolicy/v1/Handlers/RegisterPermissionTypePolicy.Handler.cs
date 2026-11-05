using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Handlers
{
public class RegisterPermitTypePolicyHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<RegisterPermissionTypePolicyCommand> logger): BaseValidatorHandler<RegisterPermissionTypePolicyCommand,Unit>(_unitOfWork, _errorManager)
{
   public override async Task<Unit> Handle(RegisterPermissionTypePolicyCommand req, CancellationToken cancellationToken)
   {
      var access = await ValidateAccessAsync(req.UserId,req.CompanyId,req.ModuleCode,cancellationToken);
      
      if (!access.IsSuccess)
      {
         return access.ErrorResponse!;
      }

      if(access.Role?.RoleType != RoleType.Administrator)
      {
        return _errorManager.ThrowBadRequest<Unit>("No tiene permiso para registrar un tipo de politica para permisos","ERP:01");
      }
      
      var exists = await _unitOfWork.PermitTypePolicyRepository.Entities
         .AnyAsync(policy=> 
         policy.CompanyId == req.CompanyId &&
         policy.PermitApplicationType == req.PermitApplicationType &&
         policy.BeneficiaryScope == req.PermitBeneficiaryScope && 
         policy.FamilyRelationship == req.FamilyRelationship,cancellationToken);
         
      if (exists)
      {
         return _errorManager.ThrowBadRequest<Unit>("Ya existe una politica para este tipo de permiso","ERP:02");
      }

      logger.LogInformation("Registrando politica de permiso {Type}", req.PermitApplicationType);

      await _unitOfWork.PermitTypePolicyRepository.RegisterPermitTypePolicy(new()
      {
         CompanyId = req.CompanyId,
         PermitApplicationType = req.PermitApplicationType,
         BeneficiaryScope = req.PermitBeneficiaryScope,
         FamilyRelationship = req.FamilyRelationship,
         ConsumesVacationBalance = req.ConsumesVacationBalance,
         RequiresEvidence = req.RequiresEvidence,
         MinEvidenceCount = req.RequiresEvidence ? req.MinEvidenceCount : 0,
         MaxEvidenceCount = req.RequiresEvidence ? req.MaxEvidenceCount : 0,
         RequiredApprovalSteps = req.RequiredApprovalSteps,
         MaxAmountDays = req.MaxAmountDays,
         IsActive = req.IsActive 
      });

         await _unitOfWork.SaveChangesAsync(cancellationToken);

         return Unit.Value; 
       }
   } 
}