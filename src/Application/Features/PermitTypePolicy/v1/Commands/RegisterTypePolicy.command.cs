using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.commands
{
   public class RegisterPermissionTypePolicyCommand : BaseRequest, IRequest<Unit>
   {
      public PermitApplicationType PermitApplicationType {get; set;}

      public FamilyRelationship FamilyRelationship {get; set;} 

      public PermitBeneficiaryScope PermitBeneficiaryScope {get; set;}
 
      public decimal? MaxAmountDays {get; set;}
      public int MaxEvidenceCount {get; set;}
      public int MinEvidenceCount {get; set;}

      public bool IsPaid {get; set;}

      public bool IsActive {get; set;}

      public bool RequiresEvidence {get; set;}

      public bool ConsumesVacationBalance {get; set;}

      public int RequiredApprovalSteps {get; set;}
   }
}