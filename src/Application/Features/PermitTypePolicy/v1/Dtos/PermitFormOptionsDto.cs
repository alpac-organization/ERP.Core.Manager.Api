namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Dtos
{ 
   public class PermitFormOptionsDto
   {
     public List<EnumOptionsDtos> PermitTypes {get; set;} = []; 
     public List<EnumOptionsDtos> BeneficiaryScopes {get; set;} = [];

     public List<EnumOptionsDtos> FamilyRelationships {get; set;} = [];
   }

   public class EnumOptionsDtos
   {
      public string Name {get; set;} = string.Empty;
      public int Value {get; set;}
   }
}
