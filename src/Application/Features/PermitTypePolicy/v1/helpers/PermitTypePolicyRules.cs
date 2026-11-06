using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1
{
   public static class PermitTypePolicyRules
   {
      public static readonly PermitApplicationType[] AllowedTypes =
      [
         PermitApplicationType.JudicialAppointment,
         PermitApplicationType.MarriageLeave,
         PermitApplicationType.BirthLeave,
         PermitApplicationType.BereavementLeave
      ];

      public static bool IsAllowed(PermitApplicationType type) => AllowedTypes.Contains(type);
   }
}
