using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings 
{
   public static class PermitOptionsLabels
   {
      private static readonly Dictionary<PermitApplicationType, string> types = new()
      {
         [PermitApplicationType.Vacation] = "Vacaciones",
         [PermitApplicationType.VacationPay] = "Pago de Vacaciones",
         [PermitApplicationType.MedicalAppointment] = "Cita Medica",
         [PermitApplicationType.DonatedVacations] = "Donacion de Vacaciones",
         [PermitApplicationType.BereavementLeave] = "Fallecimiento",
         [PermitApplicationType.BirthLeave] = "Nacimiento",
         [PermitApplicationType.MarriageLeave] = "Matrimonio",
         [PermitApplicationType.JudicialAppointment] = "Cita Judicial"
      };

      private static readonly Dictionary<PermitBeneficiaryScope,string> beneficiaryScope = new()
      {
         [PermitBeneficiaryScope.None] = "Ninguno",
         [PermitBeneficiaryScope.Own] = "Propio",
         [PermitBeneficiaryScope.FamilyNucleus] = "Núcleo familiar"
      };

      private static readonly Dictionary<FamilyRelationship,string> familyRelationship = new()
      {
         [FamilyRelationship.Child] = "Hijo",
         [FamilyRelationship.None] = "Ninguno",
         [FamilyRelationship.Parent] = "Padre",
         [FamilyRelationship.Spouse] = "Conyugue",
         [FamilyRelationship.Sibling] = "Hermano",
      };

      public static string For(PermitApplicationType permitApplicationType)
      => types.TryGetValue(permitApplicationType, out var label) ? label : permitApplicationType.ToString();

      public static string For(PermitBeneficiaryScope permitBeneficiaryScope)
      => beneficiaryScope.TryGetValue(permitBeneficiaryScope, out var label) ? label : permitBeneficiaryScope.ToString();

      public static string For(FamilyRelationship relationship)
      => familyRelationship.TryGetValue(relationship, out var label) ? label : relationship.ToString();

   }
}