using System.Collections.Frozen;

namespace ERP.Core.Manager.Api.Application.Commons.Helpers
{
   public static class ExcludedFaultCatalogs
   {
      // Exlusión de catalogos de faltas (estas se registran de manera automaticas a través de Marcación Biométrica)
      public static readonly FrozenDictionary<string, string[]> Codes =
         new Dictionary<string, string[]>
         {
            ["ALP"] = ["FL-ALP-01", "FL-ALP-03", "FMG-ALP-04"],
            ["AVS"] = ["FL-AVS-01", "FL-AVS-03", "FMG-AVS-04"],
            ["TMN"] = ["FL-TMN-01", "FL-TMN-03", "FMG-TMN-04"],
            ["VGM"] = ["FL-VGM-01", "FL-VGM-03", "FMG-VGM-04"],
            ["AMS"] = ["FL-AMS-01", "FL-AMS-03", "FMG-AMS-04"],
         }.ToFrozenDictionary();

      public static bool IsExcluded(string? companyCode, string? faultCode) =>
         !string.IsNullOrWhiteSpace(companyCode)
         && !string.IsNullOrWhiteSpace(faultCode)
         && Codes.TryGetValue(companyCode, out var codes)
         && codes.Contains(faultCode);
   }
}