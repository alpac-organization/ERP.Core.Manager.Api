using System.Text.Json;
using ERP.Core.Application.Commons.Interfaces.AWS;
using ERP.Core.Application.Commons.Utils;

namespace ERP.Core.Manager.Api.Application.Commons.Helpers
{
   public static class DisciplinaryEvidenceHelper
   {
      public static async Task<string> BuildEvidenceJsonAsync(
         IS3StorageService s3,
         IEnumerable<EvidenceInput>? evidences,
         string section,
         CancellationToken ct = default)
      {
         var items = evidences?
            .Where(e => !string.IsNullOrWhiteSpace(e.ImageBase64))
            .ToList() ?? [];

         if (items.Count == 0)
            return "{}";

         var stored = new List<EvidenceDto>();

         foreach (var evidence in items)
         {
            var url = await s3.UploadImageAsync(
               "Manager",
               section,
               evidence.ImageBase64!,
               ct);

            stored.Add(new EvidenceDto
            {
               FileName = evidence.FileName,
               ContentType = evidence.ContentType,
               ImageUrl = url
            });
         }

         return JsonSerializer.Serialize(stored, JsonManager.SnakeCaseOptions);
      }

      public static List<EvidenceDto> Deserialize(string? json)
      {
         if (string.IsNullOrWhiteSpace(json) || json == "{}")
            return [];

         try
         {
            return JsonSerializer.Deserialize<List<EvidenceDto>>(json, JsonManager.SnakeCaseOptions) ?? [];
         }
         catch (JsonException)
         {
            return [];
         }
      }
   }

   public class EvidenceInput
   {
      public string? FileName { get; set; }
      public string? ImageBase64 { get; set; }
      public string? ContentType { get; set; }
   }

   public class EvidenceDto
   {
      public string? FileName { get; set; }
      public string? ContentType { get; set; }
      public string? ImageUrl { get; set; }
   }
}