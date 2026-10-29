using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Dtos
{
   public class PermitTypePolicyDto
   {
      public Guid PermitTypePolicyId {get; set;}

      public PermitApplicationType PermitApplicationType {get; set;}

      public string PermitApplicationTypeLabel  {get; set;} = string.Empty;
      public PermitBeneficiaryScope PermitBeneficiaryScope  {get; set;}
      public string PermitBeneficiaryLabel {get; set;} = string.Empty;

      public FamilyRelationship FamilyRelationship {get; set;}
      
      public string FamilyRelationshipLabel {get; set;} = string.Empty;

      public decimal? MaxAmountDays {get; set;}
      public int MaxEvidenceCount {get; set;}
      public int MinEvidenceCount {get; set;}

      public bool IsPaid {get; set;}

      public bool IsActive {get; set;}

      public bool ConsumesVacationBalance {get; set;}

      public int RequiredApprovalSteps {get; set;}
   }
}