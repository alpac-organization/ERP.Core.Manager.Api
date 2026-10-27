using System.Text.Json;
using ERP.Core.Application.Commons.Interfaces.AWS;

namespace ERP.Core.Manager.Api.Application.Commons.Helpers
{
   public static class DisciplinaryEvidenceHelper
   {
      private static readonly JsonSerializerOptions JsonOptions = new()
      {
         PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
         PropertyNameCaseInsensitive = true
      };

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

         return JsonSerializer.Serialize(stored, JsonOptions);
      }

      public static List<EvidenceDto> Deserialize(string? json)
      {
         if (string.IsNullOrWhiteSpace(json) || json == "{}")
            return [];

         try
         {
            return JsonSerializer.Deserialize<List<EvidenceDto>>(json, JsonOptions) ?? [];
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